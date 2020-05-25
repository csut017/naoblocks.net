using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NaoBlocks.Core.Models;
using NaoBlocks.Web.Indexes;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Controllers
{
    [Route("api/v1/robots/{robotId}/[controller]")]
    [ApiController]
    [Authorize]
    public class LogsController : ControllerBase
    {
        private readonly ILogger<LogsController> _logger;
        private readonly IAsyncDocumentSession session;

        public LogsController(ILogger<LogsController> logger, IAsyncDocumentSession session)
        {
            this._logger = logger;
            this.session = session;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Dtos.RobotLog>> GetLog(string robotId, string id)
        {
            if (!long.TryParse(id, out long conversationId))
            {
                return BadRequest(new
                {
                    error = "Invalid id"
                });
            }
            this._logger.LogDebug($"Retrieving log: id {id} for robot {robotId}");
            var log = await this.session.Query<RobotLogByMachineName.Result, RobotLogByMachineName>()
                .Where(rl => rl.MachineName == robotId && rl.ConversationId == conversationId)
                .OfType<RobotLog>()
                .FirstOrDefaultAsync();
            if (log == null)
            {
                return NotFound();
            }

            this._logger.LogDebug("Retrieved robot log");
            var conversation = await this.session
                .Query<Conversation>()
                .FirstOrDefaultAsync(c => c.ConversationId == log.Conversation.ConversationId);
            return Dtos.RobotLog.FromModel(log, true, conversation);
        }

        [HttpGet]
        public async Task<Dtos.ListResult<Dtos.RobotLog>> GetLogs(string robotId, int? page, int? size)
        {
            var pageSize = size ?? 25;
            var pageNum = page ?? 0;
            if (pageSize > 100) pageSize = 100;

            this._logger.LogDebug($"Retrieving logs for {robotId}: page {pageNum} with size {pageSize}");
            var logs = await this.session.Query<RobotLogByMachineName.Result, RobotLogByMachineName>()
                                             .Statistics(out QueryStatistics stats)
                                             .Where(rl => rl.MachineName == robotId)
                                             .OfType<RobotLog>()
                                             .OrderByDescending(rl => rl.WhenAdded)
                                             .Skip(pageNum * pageSize)
                                             .Take(pageSize)
                                             .ToListAsync();
            var count = logs.Count;
            this._logger.LogDebug($"Retrieved {count} logs");
            var result = new Dtos.ListResult<Dtos.RobotLog>
            {
                Count = stats.TotalResults,
                Page = pageNum,
                Items = logs.Select(rl => Dtos.RobotLog.FromModel(rl, false, null))
            };
            return result;
        }
    }
}