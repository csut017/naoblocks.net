using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NaoBlocks.Core.Commands;
using NaoBlocks.Core.Models;
using NaoBlocks.Web.Helpers;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class RobotsController : ControllerBase
    {
        private readonly ILogger<RobotsController> _logger;
        private readonly ICommandManager commandManager;
        private readonly IAsyncDocumentSession session;

        public RobotsController(ILogger<RobotsController> logger, ICommandManager commandManager, IAsyncDocumentSession session)
        {
            this._logger = logger;
            this.commandManager = commandManager;
            this.session = session;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Dtos.ExecutionResult>> Delete(string id)
        {
            this._logger.LogInformation($"Deleting robot '{id}'");
            var command = new DeleteRobotCommand
            {
                MachineName = id
            };
            return await this.commandManager.ExecuteForHttp(command);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Dtos.Robot>> GetRobot(string id)
        {
            this._logger.LogDebug($"Retrieving robot: id {id}");
            var robot = await this.session.Query<Robot>()
                                            .Where(u => u.MachineName == id)
                                            .FirstOrDefaultAsync();
            if (robot == null)
            {
                return NotFound();
            }

            this._logger.LogDebug("Retrieved robot");
            return new Dtos.Robot
            {
                MachineName = robot.MachineName,
                FriendlyName = robot.FriendlyName
            };
        }

        [HttpGet]
        public async Task<Dtos.ListResult<Dtos.Robot>> GetRobots(int? page, int? size)
        {
            var pageSize = size ?? 25;
            var pageNum = page ?? 0;
            if (pageSize > 100) pageSize = 100;

            this._logger.LogDebug($"Retrieving robots: page {pageNum} with size {pageSize}");
            var robots = await this.session.Query<Robot>()
                                             .Statistics(out QueryStatistics stats)
                                             .OrderBy(s => s.MachineName)
                                             .Skip(pageNum * pageSize)
                                             .Take(pageSize)
                                             .ToListAsync();
            var count = robots.Count;
            this._logger.LogDebug($"Retrieved {count} robots");
            var result = new Dtos.ListResult<Dtos.Robot>
            {
                Count = stats.TotalResults,
                Page = pageNum,
                Items = robots.Select(s => new Dtos.Robot
                {
                    MachineName = s.MachineName,
                    FriendlyName = s.FriendlyName
                })
            };
            return result;
        }

        [HttpPost]
        public async Task<ActionResult<Dtos.ExecutionResult>> Post(Dtos.Robot robot)
        {
            if (robot == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing robot details"
                });
            }

            this._logger.LogInformation($"Adding new robot '{robot.MachineName}'");
            var command = new AddRobotCommand
            {
                MachineName = robot.MachineName,
                FriendlyName = robot.FriendlyName
            };
            return await this.commandManager.ExecuteForHttp(command);
        }
    }
}