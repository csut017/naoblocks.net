using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NaoBlocks.Core.Models;
using NaoBlocks.Web.Helpers;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System.Linq;
using System.Threading.Tasks;

using Commands = NaoBlocks.Core.Commands;
using Generators = NaoBlocks.Core.Generators;

namespace NaoBlocks.Web.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class RobotsController : ControllerBase
    {
        private readonly ILogger<RobotsController> _logger;
        private readonly Commands.ICommandManager commandManager;
        private readonly IAsyncDocumentSession session;

        public RobotsController(ILogger<RobotsController> logger, Commands.ICommandManager commandManager, IAsyncDocumentSession session)
        {
            this._logger = logger;
            this.commandManager = commandManager;
            this.session = session;
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult<Dtos.ExecutionResult>> Delete(string id)
        {
            this._logger.LogInformation($"Deleting robot '{id}'");
            var command = new Commands.DeleteRobot
            {
                MachineName = id
            };
            return await this.commandManager.ExecuteForHttp(command);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Dtos.Robot>> GetRobot(string id)
        {
            this._logger.LogDebug($"Retrieving robot: id {id}");
            var queryable = this.session.Query<Robot>()
                .Include<Robot>(r => r.RobotTypeId);
            var robot = await queryable.FirstOrDefaultAsync(u => u.MachineName == id);
            if (robot == null)
            {
                return NotFound();
            }

            robot.Type = await session.LoadAsync<RobotType>(robot.RobotTypeId);
            this._logger.LogDebug("Retrieved robot");
            return Dtos.Robot.FromModel(robot);
        }

        [HttpGet]
        public async Task<ActionResult<Dtos.ListResult<Dtos.Robot>>> GetRobots(int? page, int? size, string? type)
        {
            var pageSize = size ?? 25;
            var pageNum = page ?? 0;
            if (pageSize > 100) pageSize = 100;

            this._logger.LogDebug($"Retrieving robots: page {pageNum} with size {pageSize}");
            var query = this.session.Query<Robot>()
                .Include<Robot>(r => r.RobotTypeId)
                .Statistics(out QueryStatistics stats)
                .OrderBy(s => s.MachineName);
            if (!string.IsNullOrEmpty(type))
            {
                var robotType = await this.session.Query<RobotType>()
                    .FirstOrDefaultAsync(rt => rt.Name == type);
                if (robotType == null)
                {
                    return NotFound();
                }

                query.Where(r => r.RobotTypeId == robotType.Id);
            }

            var robots = await query
                .Skip(pageNum * pageSize)
                .Take(pageSize).ToListAsync();
            robots.ForEach(async r =>
            {
                r.Type = string.IsNullOrEmpty(r.RobotTypeId)
                    ? RobotType.Unknown
                    : await session.LoadAsync<RobotType>(r.RobotTypeId);
            });
            var count = robots.Count;
            this._logger.LogDebug($"Retrieved {count} robots");
            var result = new Dtos.ListResult<Dtos.Robot>
            {
                Count = stats.TotalResults,
                Page = pageNum,
                Items = robots.Select(Dtos.Robot.FromModel)
            };
            return result;
        }

        [HttpPost]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult<Dtos.ExecutionResult<Dtos.Robot>>> Post(Dtos.Robot robot)
        {
            if (robot == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing robot details"
                });
            }

            this._logger.LogInformation($"Adding new robot '{robot.MachineName}'");
            var command = new Commands.AddRobot
            {
                MachineName = robot.MachineName,
                FriendlyName = robot.FriendlyName,
                Password = robot.Password,
                Type = robot.Type
            };
            return await this.commandManager.ExecuteForHttp(command, Dtos.Robot.FromModel);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult<Dtos.ExecutionResult>> Put(string? id, Dtos.Robot? robot)
        {
            if ((robot == null) || string.IsNullOrEmpty(id))
            {
                return this.BadRequest(new
                {
                    Error = "Missing robot details"
                });
            }

            this._logger.LogInformation($"Updating robot '{id}'");
            var command = new Commands.UpdateRobot
            {
                CurrentMachineName = id,
                MachineName = robot.MachineName,
                FriendlyName = robot.FriendlyName,
                Password = robot.Password,
                Type = robot.Type
            };
            return await this.commandManager.ExecuteForHttp(command);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<Dtos.ExecutionResult<Dtos.Robot>>> Register(Dtos.Robot robot)
        {
            if (robot == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing robot details"
                });
            }

            this._logger.LogInformation($"Registering new robot '{robot.MachineName}'");
            var command = new Commands.RegisterRobot
            {
                MachineName = robot.MachineName
            };
            return await this.commandManager.ExecuteForHttp(command, Dtos.Robot.FromModel);
        }

        [HttpGet("export/list")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult> ExportList()
        {
            var excelData = await Generators.RobotsList.GenerateAsync(this.session);
            var contentType = ContentTypes.Xlsx;
            var fileName = "Robots-List.xlsx";
            return File(excelData, contentType, fileName);
        }
    }
}