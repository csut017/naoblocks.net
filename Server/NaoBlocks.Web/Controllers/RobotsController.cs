using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Helpers;
using Data = NaoBlocks.Engine.Data;
using Generators = NaoBlocks.Engine.Generators;
using Transfer = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Controllers
{
    /// <summary>
    /// A controller for working with robots.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
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

        /// <summary>
        /// Generates the robot list export.
        /// </summary>
        /// <param name="format">The format to use.</param>
        /// <returns>The generated robot list.</returns>
        [HttpGet("export/list")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult> ExportList(string? format)
        {
            return await this.GenerateReport<Generators.RobotsList>(
                this.executionEngine,
                format);
        }

        /// <summary>
        /// Retrieves a robot by its machine name.
        /// </summary>
        /// <param name="name">The name of the robot.</param>
        /// <returns>Either a 404 (not found) or the robot details.</returns>
        [HttpGet("{name}")]
        public async Task<ActionResult<Transfer.Robot>> Get(string name)
        {
            this._logger.LogDebug($"Retrieving robot: id {name}");
            var robot = await this.executionEngine
                .Query<RobotData>()
                .RetrieveByNameAsync(name, true)
                .ConfigureAwait(false);
            if (robot == null)
            {
                return NotFound();
            }

            this._logger.LogDebug("Retrieved robot");
            return Transfer.Robot.FromModel(robot);
        }

        /// <summary>
        /// Retrieves a page of robots.
        /// </summary>
        /// <param name="page">The page number.</param>
        /// <param name="size">The number of records.</param>
        /// <param name="type">The type of robot to retrieve.</param>
        /// <returns>A <see cref="ListResult{TData}"/> containing the robots.</returns>
        [HttpGet]
        public async Task<ActionResult<ListResult<Transfer.Robot>>> List(int? page, int? size, string? type)
        {
            (int pageNum, int pageSize) = this.ValidatePageArguments(page, size);
            this._logger.LogDebug($"Retrieving robots: page {pageNum} with size {pageSize}");
            string? typeFilter = null;

            if (!string.IsNullOrEmpty(type))
            {
                var robotType = await this.executionEngine
                    .Query<RobotTypeData>()
                    .RetrieveByNameAsync(type)
                    .ConfigureAwait(false);
                if (robotType == null)
                {
                    return NotFound();
                }

                typeFilter = robotType.Id;
            }

            var robots = await this.executionEngine
                .Query<RobotData>()
                .RetrievePageAsync(pageNum, pageSize, typeFilter)
                .ConfigureAwait(false);
            var count = robots.Items?.Count();
            this._logger.LogDebug($"Retrieved {count} robots");
            var result = new ListResult<Transfer.Robot>
            {
                Count = robots.Count,
                Page = pageNum,
                Items = robots.Items?.Select(r => Transfer.Robot.FromModel(r))
            };
            return result;
        }

        /// <summary>
        /// Adds a new robot.
        /// </summary>
        /// <param name="robot">The robot to add.</param>
        /// <returns>The result of execution.</returns>
        [HttpPost]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult<ExecutionResult<Transfer.Robot>>> Post(Transfer.Robot? robot)
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
            return await this.executionEngine
                .ExecuteForHttp<Data.Robot, Transfer.Robot>
                (command, r => Transfer.Robot.FromModel(r!));
        }

        /// <summary>
        /// Updates an existing robot.
        /// </summary>
        /// <param name="id">The machine name of the robot.</param>
        /// <param name="robot">The details of the robot.</param>
        /// <returns>The result of execution.</returns>
        [HttpPut("{id}")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult<ExecutionResult<Transfer.Robot>>> Put(string? id, Transfer.Robot? robot)
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
                MachineName = id,
                FriendlyName = robot.FriendlyName,
                Password = robot.Password,
                RobotType = robot.Type
            };
            return await this.executionEngine
                .ExecuteForHttp<Data.Robot, Transfer.Robot>
                (command, r => Transfer.Robot.FromModel(r!));
        }

        /// <summary>
        /// Registers a new unknown robot.
        /// </summary>
        /// <param name="robot">The robot details</param>
        /// <returns>The result of execution.</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<ExecutionResult<Transfer.Robot>>> Register(Transfer.Robot? robot)
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
            return await this.executionEngine
                .ExecuteForHttp<Data.Robot, Transfer.Robot>
                (command, r => Transfer.Robot.FromModel(r!));
        }
    }
}