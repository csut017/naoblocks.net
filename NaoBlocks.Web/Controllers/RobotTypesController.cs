using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NaoBlocks.Core.Generators;
using NaoBlocks.Core.Models;
using NaoBlocks.Web.Helpers;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Commands = NaoBlocks.Core.Commands;
using Generators = NaoBlocks.Core.Generators;

namespace NaoBlocks.Web.Controllers
{
    [Route("api/v1/robots/types")]
    [ApiController]
    [Authorize]
    public class RobotTypesController : ControllerBase
    {
        private readonly ILogger<RobotTypesController> _logger;
        private readonly Commands.ICommandManager commandManager;
        private readonly IAsyncDocumentSession session;
        private readonly string rootFolder;

        public RobotTypesController(ILogger<RobotTypesController> logger, Commands.ICommandManager commandManager, IAsyncDocumentSession session, IHostingEnvironment hostingEnvironment)
        {
            this._logger = logger;
            this.commandManager = commandManager;
            this.session = session;
            this.rootFolder = hostingEnvironment.WebRootPath;
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult<Dtos.ExecutionResult>> Delete(string id)
        {
            this._logger.LogInformation($"Deleting robot type '{id}'");
            var command = new Commands.DeleteRobotType
            {
                Name = id
            };
            return await this.commandManager.ExecuteForHttp(command);
        }

        [HttpPost("{id}/files")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult> RetrieveFileList(string id, string? generate)
        {
            this._logger.LogDebug($"Retrieving robot type: {id}");
            var queryable = this.session.Query<RobotType>();
            var robotType = await queryable.FirstOrDefaultAsync(u => u.Name == id);
            if (robotType == null)
            {
                return NotFound();
            }

            byte[] fileList;
            if ("full".Equals(generate, StringComparison.OrdinalIgnoreCase))
            {
                this._logger.LogInformation($"Generating file list for robot type '{id}'");
                fileList = await RobotTypeFilePackage.GenerateListAsync(robotType, this.rootFolder);
            }
            else
            {
                this._logger.LogInformation($"Retrieving file list for robot type '{id}'");
                fileList = await RobotTypeFilePackage.RetrieveListAsync(robotType, this.rootFolder);
            }


            return File(fileList, ContentTypes.Txt, "filelist.txt");
        }

        [HttpGet("export/package/{id}")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult> ExportPackage(string id)
        {
            this._logger.LogDebug($"Generating package for robot type: {id}");
            var queryable = this.session.Query<RobotType>();
            var robotType = await queryable.FirstOrDefaultAsync(u => u.Name == id);
            if (robotType == null)
            {
                return NotFound();
            }

            var stream = await Generators.RobotTypePackage.GeneraAsync(robotType, this.session);
            var contentType = ContentTypes.Xlsx;
            var fileName = $"RobotType-{robotType.Name}-Package.zip";
            this._logger.LogDebug($"Generated robot type {robotType.Name} package");
            return File(stream, contentType, fileName);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Dtos.RobotType>> GetRobotType(string id)
        {
            this._logger.LogDebug($"Retrieving robot type: {id}");
            var queryable = this.session.Query<RobotType>();
            var robotType = await queryable.FirstOrDefaultAsync(u => u.Name == id);
            if (robotType == null)
            {
                return NotFound();
            }

            this._logger.LogDebug($"Retrieved robot type ${robotType.Name}");
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
            var command = new Commands.AddRobotType
            {
                Name = robotType.Name,
                IsDefault = robotType.IsDefault.GetValueOrDefault(false)
            };
            return await this.commandManager.ExecuteForHttp(command, Dtos.RobotType.FromModel);
        }

        [HttpPost("{id}/toolbox")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult<Dtos.ExecutionResult<Dtos.RobotType>>> ImportToolbox(string? id)
        {
            var xml = string.Empty;
            using (var reader = new StreamReader(this.Request.Body))
            {
                xml = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(xml))
            {
                return this.BadRequest(new
                {
                    Error = "Missing toolbox definition"
                });
            }

            this._logger.LogInformation($"Updating robot type '{id}'");
            var command = new Commands.ImportToolbox
            {
                Name = id,
                Definition = xml
            };
            return await this.commandManager.ExecuteForHttp(command, rt => Dtos.RobotType.FromModel(rt, Dtos.ConversionOptions.IncludeDetails));
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult<Dtos.ExecutionResult<Dtos.RobotType>>> Put(string? id, Dtos.RobotType? robotType)
        {
            if ((robotType == null) || string.IsNullOrEmpty(id))
            {
                return this.BadRequest(new
                {
                    Error = "Missing robot type details"
                });
            }

            this._logger.LogInformation($"Updating robot type '{id}'");
            var command = new Commands.UpdateRobotType
            {
                CurrentName = id,
                Name = robotType.Name,
                IsDefault = robotType.IsDefault
            };
            return await this.commandManager.ExecuteForHttp(command, Dtos.RobotType.FromModel);
        }
    }
}