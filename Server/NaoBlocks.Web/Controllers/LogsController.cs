using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Helpers;
using System.Diagnostics;

namespace NaoBlocks.Web.Controllers
{
    /// <summary>
    /// A controller for working with robot logs.
    /// </summary>
    [Route("api/v1/robots/{robotId}/[controller]")]
    [ApiController]
    [Authorize]
    [Authorize(Policy = "Teacher")]
    [Produces("application/json")]
    public class LogsController : ControllerBase
    {
        private readonly IExecutionEngine executionEngine;
        private readonly ILogger<LogsController> logger;

        /// <summary>
        /// Initialises a new <see cref="LogsController"/> instance.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="executionEngine">The execution engine for processing commands and queries.</param>
        public LogsController(ILogger<LogsController> logger, IExecutionEngine executionEngine)
        {
            this.logger = logger;
            this.executionEngine = executionEngine;
        }

        /// <summary>
        /// Retrieves a robot log by its conversation id.
        /// </summary>
        /// <param name="robotId">The identifier of the robot.</param>
        /// <param name="id">The conversation id.</param>
        /// <returns>Either a 404 (not found) or the log details.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Dtos.RobotLog>> Get(string robotId, string id)
        {
            if (!long.TryParse(id, out long conversationId))
            {
                return BadRequest(new
                {
                    error = "Invalid id"
                });
            }

            this.logger.LogDebug("Retrieving log: id {id} for robot {robotId}", id, robotId);
            var log = await this.executionEngine
                .Query<ConversationData>()
                .RetrieveRobotLogAsync(conversationId, robotId);
            if (log == null)
            {
                return NotFound();
            }

            this.logger.LogDebug("Retrieved robot log");
            return Dtos.RobotLog.FromModel(log, true);
        }

        /// <summary>
        /// Retrieves the logs for a robot.
        /// </summary>
        /// <param name="robotId">The identifier of the robot.</param>
        /// <param name="page">The page number.</param>
        /// <param name="size">The number of records.</param>
        /// <returns>Either a 404 (not found) or the page of logs for the robot.</returns>
        [HttpGet]
        public async Task<ActionResult<ListResult<Dtos.RobotLog>>> GetLogs(string robotId, int? page, int? size)
        {
            this.logger.LogDebug("Retrieving robot: id {robotId}", robotId);
            var robot = await this.executionEngine
                .Query<RobotData>()
                .RetrieveByNameAsync(robotId, false)
                .ConfigureAwait(false);
            if (robot == null)
            {
                return NotFound();
            }

            (int pageNum, int pageSize) = this.ValidatePageArguments(page, size);
            this.logger.LogDebug("Retrieving logs for {machineName}: page {pageNum} with size {pageSize}", robot.MachineName, pageNum, pageSize);

            var logs = await this.executionEngine
                .Query<ConversationData>()
                .RetrieveRobotLogsPageAsync(robotId, pageNum, pageSize)
                .ConfigureAwait(false);
            var count = logs.Items?.Count();
            this.logger.LogDebug("Retrieved {count} logs", count);

            var result = new ListResult<Dtos.RobotLog>
            {
                Count = logs.Count,
                Page = pageNum,
                Items = logs.Items?.Select(r => Dtos.RobotLog.FromModel(r, false))
            };
            return result;
        }

        /// <summary>
        /// Records a logging request.
        /// </summary>
        /// <param name="robotId">The identifier of the robot.</param>
        /// <param name="request">The request containing the log messages.</param>
        /// <returns>A <see cref="Dtos.LogResult"/> containing the outcome of the operation.</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<Dtos.LogResult>> Post(string robotId, Dtos.LogRequest request)
        {
            this.logger.LogDebug("Retrieving robot: id {robotId}", robotId);
            var robot = await this.executionEngine
                .Query<RobotData>()
                .RetrieveByNameAsync(robotId, true)
                .ConfigureAwait(false);
            if (robot == null)
            {
                return NotFound();
            }

            if (!robot.Type?.AllowDirectLogging ?? false)
            {
                this.logger.LogWarning("Robot type {name} does not allow direct logging", robot.Type?.Name);
                return Forbid();
            }

            switch (request.Action?.ToLowerInvariant() ?? "none")
            {
                case "init":
                    return await this.InitialiseRobotLog(robot, request);

                case "log":
                    return await this.AddLogMessages(robot, request);

                default:
                    return new Dtos.LogResult { Error = $"Unknown action '{request.Action}'" };
            }
        }

        /// <summary>
        /// Adds a batch of messages.
        /// </summary>
        /// <param name="robot">The <see cref="Robot"/> to add the messages for.</param>
        /// <param name="request">The <see cref="Dtos.LogRequest"/> containing the messages.</param>
        /// <returns>A <see cref="Dtos.LogResult"/> containing the outcome.</returns>
        private async Task<ActionResult<Dtos.LogResult>> AddLogMessages(Robot robot, Dtos.LogRequest request)
        {
            var batch = new Batch();
            var now = DateTime.UtcNow
                              .AddSeconds(-request.Time.GetValueOrDefault(0.0));

            if ((request.Messages == null) || !request.Messages.Any())
            {
                return new Dtos.LogResult();
            }

            foreach (var message in request.Messages!)
            {
                var addLog = new AddToRobotLog
                {
                    Description = message,
                    MachineName = robot.MachineName,
                    UseLastConversationId = true,
                    SourceMessageType = ClientMessageType.RobotDebugMessage
                };
                var parts = message.Split(':');
                if (parts.Length > 1)
                {
                    var timeOffset = double.Parse(parts[0]);
                    var messageTime = now.AddSeconds(timeOffset);
                    addLog.Values.Add(new NamedValue { Name = "RobotTime", Value = messageTime.ToString("O") });
                    addLog.Description = parts[1];
                }

                batch.Commands.Add(addLog);
            }

            var result = await ExecuteCommand(batch);
            return result.Item1;
        }

        /// <summary>
        /// Executes a command.
        /// </summary>
        /// <param name="command">The <see cref="CommandBase"/> to execute.</param>
        /// <param name="commit">Whether to commit the changes or not.</param>
        /// <returns>A <see cref="Dtos.LogResult"/> and <see cref="CommandResult"/> containing the result of execution.</returns>
        private async Task<(Dtos.LogResult, CommandResult?)> ExecuteCommand(CommandBase command, bool commit = true)
        {
            var commandName = command.GetType().Name;
            this.executionEngine.Logger.LogDebug("Validating {command} command", commandName);
            var errors = await this.executionEngine.ValidateAsync(command);
            if (errors.Any())
            {
                this.executionEngine.LogValidationFailure(command, errors);
                var allErrors = string.Join(", ", errors.Select(e => e.Error));
                return (new Dtos.LogResult { Error = $"Command is invalid: {allErrors}" }, null);
            }

            this.executionEngine.Logger.LogDebug("Executing {command} command", commandName);
            var outcome = await this.executionEngine.ExecuteAsync(command);
            if (!outcome.WasSuccessful)
            {
                this.executionEngine.LogExecutionFailure(command, outcome);
                return (new Dtos.LogResult { Error = $"Command failed: {outcome.Error}" }, null);
            }

            if (commit) await this.executionEngine.CommitAsync();
            this.executionEngine.Logger.LogInformation("Executed {command} successfully", commandName);

            return (new Dtos.LogResult(), outcome);
        }

        /// <summary>
        /// Initialises a robot for logging.
        /// </summary>
        /// <param name="robot">The <see cref="Robot"/> to initialise the conversation for.</param>
        /// <param name="request">The <see cref="Dtos.LogRequest"/> containing the robot details.</param>
        /// <returns>A <see cref="Dtos.LogResult"/> containing the outcome.</returns>
        private async Task<ActionResult<Dtos.LogResult>> InitialiseRobotLog(Robot robot, Dtos.LogRequest request)
        {
            var startConversation = new StartRobotConversation
            {
                Name = robot.MachineName,
                Type = ConversationType.Logging
            };

            var result = await this.ExecuteCommand(startConversation, false);
            if (!string.IsNullOrEmpty(result.Item1.Error))
            {
                return result.Item1;
            }

            var conversation = result.Item2?.As<Conversation>().Output;
            if (conversation == null) throw new UnreachableException("Should always have a conversation here");
            var addLog = new AddToRobotLog
            {
                Description = "Started new session",
                MachineName = robot.MachineName,
                Conversation = conversation,
                SourceMessageType = ClientMessageType.StartProgram
            };
            if (!string.IsNullOrEmpty(request.Version)) addLog.Values.Add(new NamedValue { Name = "Version", Value = request.Version });
            result = await ExecuteCommand(addLog);
            if (string.IsNullOrEmpty(result.Item1?.Error))
            {
                var values = new Dictionary<string, string>();
                foreach (var value in robot.Type!.CustomValues)
                {
                    values[value!.Name] = value?.Value ?? string.Empty;
                }

                foreach (var value in robot.CustomValues)
                {
                    if (values.ContainsKey(value.Name))
                    {
                        values[value!.Name] = value?.Value ?? string.Empty;
                    }
                }

                result.Item1!.Values = values;
            }

            return result.Item1;
        }
    }
}