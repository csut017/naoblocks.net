using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Helpers;

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

            this.logger.LogDebug($"Retrieving log: id {id} for robot {robotId}");
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
            this.logger.LogDebug($"Retrieving robot: id {robotId}");
            var robot = await this.executionEngine
                .Query<RobotData>()
                .RetrieveByNameAsync(robotId, true)
                .ConfigureAwait(false);
            if (robot == null)
            {
                return NotFound();
            }

            (int pageNum, int pageSize) = this.ValidatePageArguments(page, size);
            this.logger.LogDebug($"Retrieving robots: page {pageNum} with size {pageSize}");

            var logs = await this.executionEngine
                .Query<ConversationData>()
                .RetrieveRobotLogsPageAsync(robotId, pageNum, pageSize)
                .ConfigureAwait(false);
            var count = logs.Items?.Count();
            this.logger.LogDebug($"Retrieved {count} logs");

            var result = new ListResult<Dtos.RobotLog>
            {
                Count = logs.Count,
                Page = pageNum,
                Items = logs.Items?.Select(r => Dtos.RobotLog.FromModel(r, false))
            };
            return result;
        }
    }
}