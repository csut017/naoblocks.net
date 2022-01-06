using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Queries;

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
        private readonly ILogger<LogsController> logger;
        private readonly IExecutionEngine executionEngine;

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

        /*
        /// <summary>
        /// Retrieves the logs for a robot.
        /// </summary>
        /// <param name="robotId">The identifier of the robot.</param>
        /// <returns>Either a 404 (not found) or the log details.</returns>
        [HttpGet]
        public async Task<ListResult<Dtos.RobotLog>> GetLogs(string robotId, int? page, int? size)
        {
            var pageSize = size ?? 25;
            var pageNum = page ?? 0;
            if (pageSize > 100) pageSize = 100;

            this.logger.LogDebug($"Retrieving logs for {robotId}: page {pageNum} with size {pageSize}");
            var logs = await this.session.Query<RobotLogByMachineName.Result, RobotLogByMachineName>()
                                             .Statistics(out QueryStatistics stats)
                                             .Where(rl => rl.MachineName == robotId)
                                             .OfType<RobotLog>()
                                             .OrderByDescending(rl => rl.WhenAdded)
                                             .Skip(pageNum * pageSize)
                                             .Take(pageSize)
                                             .ToListAsync();
            var count = logs.Count;
            this.logger.LogDebug($"Retrieved {count} logs");
            var result = new ListResult<Dtos.RobotLog>
            {
                Count = stats.TotalResults,
                Page = pageNum,
                Items = logs.Select(rl => Dtos.RobotLog.FromModel(rl, false, null))
            };
            return result;
        }
        */
    }
}
