using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Helpers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Data = NaoBlocks.Engine.Data;
using Transfer = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Controllers
{
    /// <summary>
    /// A controller for working with robots.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class RobotsController : ControllerBase
    {
        private readonly ILogger<RobotsController> _logger;
        private readonly IExecutionEngine executionEngine;

        /// <summary>
        /// Initialises a new <see cref="RobotsController"/> instance.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="executionEngine">The execution engine for processing commands and queries.</param>
        public RobotsController(ILogger<RobotsController> logger, IExecutionEngine executionEngine)
        {
            this._logger = logger;
            this.executionEngine = executionEngine;
        }

        /// <summary>
        /// Deletes a robot.
        /// </summary>
        /// <param name="id">The machine name of the robot.</param>
        /// <returns>The result of execution.</returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult<ExecutionResult>> Delete(string id)
        {
            this._logger.LogInformation($"Deleting robot '{id}'");
            var command = new DeleteRobot
            {
                Name = id
            };
            return await this.executionEngine.ExecuteForHttp(command);
        }

        /*
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
        public async Task<ActionResult<ListResult<Dtos.Robot>>> GetRobots(int? page, int? size, string? type)
        {
            var pageSize = size ?? 25;
            var pageNum = page ?? 0;
            if (pageSize > 100) pageSize = 100;

            this._logger.LogDebug($"Retrieving robots: page {pageNum} with size {pageSize}");
            List<Robot> robots;
            QueryStatistics stats;
            if (!string.IsNullOrEmpty(type))
            {
                var robotType = await this.session.Query<RobotType>()
                    .FirstOrDefaultAsync(rt => rt.Name == type);
                if (robotType == null)
                {
                    return NotFound();
                }

                robots = await this.session.Query<Robot>()
                    .Include<Robot>(r => r.RobotTypeId)
                    .Statistics(out stats)
                    .Where(r => r.RobotTypeId == robotType.Id)
                    .OrderBy(s => s.MachineName)
                    .Skip(pageNum * pageSize)
                    .Take(pageSize)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
            else
            {
                robots = await this.session.Query<Robot>()
                    .Include<Robot>(r => r.RobotTypeId)
                    .Statistics(out stats)
                    .OrderBy(s => s.MachineName)
                    .Skip(pageNum * pageSize)
                    .Take(pageSize)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }

            robots.ForEach(async r =>
            {
                r.Type = string.IsNullOrEmpty(r.RobotTypeId)
                    ? RobotType.Unknown
                    : await session.LoadAsync<RobotType>(r.RobotTypeId);
            });
            var count = robots.Count;
            this._logger.LogDebug($"Retrieved {count} robots");
            var result = new ListResult<Dtos.Robot>
            {
                Count = stats.TotalResults,
                Page = pageNum,
                Items = robots.Select(Dtos.Robot.FromModel)
            };
            return result;
        }

        [HttpPost]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult<ExecutionResult<Dtos.Robot>>> Post(Dtos.Robot robot)
        {
            if (robot == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing robot details"
                });
            }

            this._logger.LogInformation($"Adding new robot '{robot.MachineName}'");
            var command = new AddRobot
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
        public async Task<ActionResult<ExecutionResult>> Put(string? id, Dtos.Robot? robot)
        {
            if ((robot == null) || string.IsNullOrEmpty(id))
            {
                return this.BadRequest(new
                {
                    Error = "Missing robot details"
                });
            }

            this._logger.LogInformation($"Updating robot '{id}'");
            var command = new UpdateRobot
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
        public async Task<ActionResult<ExecutionResult<Dtos.Robot>>> Register(Dtos.Robot robot)
        {
            if (robot == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing robot details"
                });
            }

            this._logger.LogInformation($"Registering new robot '{robot.MachineName}'");
            var command = new RegisterRobot
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
        */
    }
}
