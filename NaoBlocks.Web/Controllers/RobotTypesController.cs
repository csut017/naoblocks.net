using Microsoft.AspNetCore.Authorization;
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
    [Route("api/v1/robots/types")]
    [ApiController]
    [Authorize]
    public class RobotTypesController : ControllerBase
    {
        private readonly ILogger<RobotTypesController> _logger;
        private readonly ICommandManager commandManager;
        private readonly IAsyncDocumentSession session;

        public RobotTypesController(ILogger<RobotTypesController> logger, ICommandManager commandManager, IAsyncDocumentSession session)
        {
            this._logger = logger;
            this.commandManager = commandManager;
            this.session = session;
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult<Dtos.ExecutionResult>> Delete(string id)
        {
            this._logger.LogInformation($"Deleting robot type '{id}'");
            var command = new DeleteRobotTypeCommand
            {
                Name = id
            };
            return await this.commandManager.ExecuteForHttp(command);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Dtos.RobotType>> GetRobotType(string id)
        {
            this._logger.LogDebug($"Retrieving robot type: id {id}");
            var queryable = this.session.Query<RobotType>();
            var robotType = await queryable.FirstOrDefaultAsync(u => u.Name == id);
            if (robotType == null)
            {
                return NotFound();
            }

            this._logger.LogDebug("Retrieved robot type");
            return Dtos.RobotType.FromModel(robotType);
        }

        [HttpGet]
        public async Task<Dtos.ListResult<Dtos.RobotType>> GetRobotTypes(int? page, int? size)
        {
            var pageSize = size ?? 25;
            var pageNum = page ?? 0;
            if (pageSize > 100) pageSize = 100;

            this._logger.LogDebug($"Retrieving robot types: page {pageNum} with size {pageSize}");
            var robotTypes = await this.session.Query<RobotType>()
                                             .Statistics(out QueryStatistics stats)
                                             .OrderBy(s => s.Name)
                                             .Skip(pageNum * pageSize)
                                             .Take(pageSize)
                                             .ToListAsync();
            var count = robotTypes.Count;
            this._logger.LogDebug($"Retrieved {count} robot types");
            var result = new Dtos.ListResult<Dtos.RobotType>
            {
                Count = stats.TotalResults,
                Page = pageNum,
                Items = robotTypes.Select(Dtos.RobotType.FromModel)
            };
            return result;
        }

        [HttpPost]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult<Dtos.ExecutionResult<Dtos.RobotType>>> Post(Dtos.RobotType robotType)
        {
            if (robotType == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing robot type details"
                });
            }

            this._logger.LogInformation($"Adding new robot type '{robotType.Name}'");
            var command = new AddRobotTypeCommand
            {
                Name = robotType.Name
            };
            return await this.commandManager.ExecuteForHttp(command, Dtos.RobotType.FromModel);
        }

        //[HttpPut("{id}")]
        //[Authorize(Policy = "Teacher")]
        //public async Task<ActionResult<Dtos.ExecutionResult>> Put(string? id, Dtos.Robot? robot)
        //{
        //    if ((robot == null) || string.IsNullOrEmpty(id))
        //    {
        //        return this.BadRequest(new
        //        {
        //            Error = "Missing robot details"
        //        });
        //    }

        //    this._logger.LogInformation($"Updating robot '{id}'");
        //    var command = new UpdateRobotCommand
        //    {
        //        CurrentMachineName = id,
        //        MachineName = robot.MachineName,
        //        FriendlyName = robot.FriendlyName,
        //        Password = robot.Password
        //    };
        //    return await this.commandManager.ExecuteForHttp(command);
        //}
    }
}