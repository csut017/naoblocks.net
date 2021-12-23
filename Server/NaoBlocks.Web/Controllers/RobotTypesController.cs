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
    /// A controller for working with robot types.
    /// </summary>
    [Route("api/v1/robots/types")]
    [ApiController]
    [Authorize]
    public class RobotTypesController : ControllerBase
    {
        private readonly ILogger<RobotTypesController> _logger;
        private readonly IExecutionEngine executionEngine;
        private readonly string rootFolder;

        /// <summary>
        /// Initialises a new <see cref="RobotTypesController"/> instance.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="executionEngine">The execution engine for processing commands and queries.</param>
        /// <param name="hostingEnvironment">The web hosting environment.</param>
        public RobotTypesController(ILogger<RobotTypesController> logger, IExecutionEngine executionEngine, IWebHostEnvironment hostingEnvironment)
        {
            this._logger = logger;
            this.executionEngine = executionEngine;
            this.rootFolder = hostingEnvironment.WebRootPath;
        }

        /// <summary>
        /// Deletes a robot type.
        /// </summary>
        /// <param name="id">The id of the robot type.</param>
        /// <returns>The result of execution.</returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult<ExecutionResult>> Delete(string id)
        {
            this._logger.LogInformation($"Deleting robot type '{id}'");
            var command = new DeleteRobotType
            {
                Name = id
            };
            return await this.executionEngine.ExecuteForHttp(command);
        }

        /// <summary>
        /// Retrieves a robot type by its name.
        /// </summary>
        /// <param name="name">The name of the robot type.</param>
        /// <returns>Either a 404 (not found) or the robot type details.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Transfer.RobotType>> Get(string id)
        {
            this._logger.LogDebug($"Retrieving robot type: {id}");
            var robotType = await this.executionEngine
                .Query<RobotTypeData>()
                .RetrieveByNameAsync(id)
                .ConfigureAwait(false);
            if (robotType == null)
            {
                return NotFound();
            }

            this._logger.LogDebug($"Retrieved robot type ${robotType.Name}");
            return Transfer.RobotType.FromModel(robotType);
        }

        /// <summary>
        /// Retrieves a page of robot types.
        /// </summary>
        /// <param name="page">The page number.</param>
        /// <param name="size">The number of records.</param>
        /// <returns>A <see cref="ListResult{TData}"/> containing the robot types.</returns>
        [HttpGet]
        public async Task<ListResult<Transfer.RobotType>> List(int? page, int? size)
        {
            (int pageNum, int pageSize) = this.ValidatePageArguments(page, size);
            this._logger.LogDebug($"Retrieving robot types: page {pageNum} with size {pageSize}");
            var robotTypes = await this.executionEngine
                .Query<RobotTypeData>()
                .RetrievePageAsync(pageNum, pageSize)
                .ConfigureAwait(false);
            var count = robotTypes.Items?.Count() ?? 0;
            this._logger.LogDebug($"Retrieved {count} robot types");
            var result = new ListResult<Transfer.RobotType>
            {
                Count = robotTypes.Count,
                Page = pageNum,
                Items = robotTypes.Items?.Select(r => Transfer.RobotType.FromModel(r))
            };
            return result;
        }

        /// <summary>
        /// Adds a new robot type.
        /// </summary>
        /// <param name="robot">The robot type to add.</param>
        /// <returns>The result of execution.</returns>
        [HttpPost]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult<ExecutionResult<Transfer.RobotType>>> Post(Transfer.RobotType? robotType)
        {
            if (robotType == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing robot type details"
                });
            }

            this._logger.LogInformation($"Adding new robot type '{robotType.Name}'");
            var command = new AddRobotType
            {
                Name = robotType.Name
            };
            return await this.executionEngine
                .ExecuteForHttp<Data.RobotType, Transfer.RobotType>(
                command,
                t => Transfer.RobotType.FromModel(t!));
        }

        /// <summary>
        /// Updates an existing robot type.
        /// </summary>
        /// <param name="id">The name of the robot type.</param>
        /// <param name="robot">The details of the robot type.</param>
        /// <returns>The result of execution.</returns>
        [HttpPut("{id}")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult<ExecutionResult<Transfer.RobotType>>> Put(string? id, Transfer.RobotType? robotType)
        {
            if ((robotType == null) || string.IsNullOrEmpty(id))
            {
                return this.BadRequest(new
                {
                    Error = "Missing robot type details"
                });
            }

            this._logger.LogInformation($"Updating robot type '{id}'");
            var command = new UpdateRobotType
            {
                CurrentName = id,
                Name = robotType.Name
            };
            return await this.executionEngine
                .ExecuteForHttp<Data.RobotType, Transfer.RobotType>(
                command,
                t => Transfer.RobotType.FromModel(t!));
        }

        /*
        /// <summary>
        /// Retrieves a package list for a robot type.
        /// </summary>
        /// <param name="name">The name of the robot type.</param>
        /// <returns>Either a 404 (not found) or the robot type details.</returns>
        [HttpGet("{id}/package{format}")]
        [HttpGet("{id}/package")]
        [AllowAnonymous]
        public async Task<ActionResult> RetrievePackageFileList(string id, string? format = ".json")
        {
            this._logger.LogDebug($"Retrieving robot type: {id}");
            var queryable = this.session.Query<RobotType>();
            var robotType = await queryable.FirstOrDefaultAsync(u => u.Name == id);
            if (robotType == null)
            {
                return NotFound();
            }

            this._logger.LogInformation($"Retrieving file list for robot type '{id}'");
            var fileList = await RobotTypeFilePackage.RetrieveListAsync(robotType, this.rootFolder);

            if (format == ".txt")
            {
                return File(fileList, ContentTypes.Txt, "filelist.txt");
            }

            var data = Encoding.UTF8.GetString(fileList);
            return new JsonResult(ListResult.New(
                data.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line =>
                    {
                        var values = line.Split(',');
                        return new Transfer.PackageFile
                        {
                            Name = values[0],
                            Hash = values[1]
                        };
                    })));
        }

        [HttpPost("{id}/package")]
        [Authorize(Policy = "Administrator")]
        public async Task<ActionResult> GeneratePackageFileList(string id)
        {
            this._logger.LogDebug($"Retrieving robot type: {id}");
            var queryable = this.session.Query<RobotType>();
            var robotType = await queryable.FirstOrDefaultAsync(u => u.Name == id);
            if (robotType == null)
            {
                return NotFound();
            }

            this._logger.LogInformation($"Generating file list for robot type '{id}'");
            var fileList = await RobotTypeFilePackage.GenerateListAsync(robotType, this.rootFolder);

            return File(fileList, ContentTypes.Txt, "filelist.txt");
        }

        [HttpPost("{id}/package/files")]
        [Authorize(Policy = "Administrator")]
        public async Task<ActionResult<ExecutionResult>> UploadPackageFile(string id, NamedValue file)
        {
            this._logger.LogDebug($"Retrieving robot type: {id}");
            var queryable = this.session.Query<RobotType>();
            var robotType = await queryable.FirstOrDefaultAsync(u => u.Name == id);
            if (robotType == null)
            {
                return NotFound();
            }

            this._logger.LogInformation($"Uploading package file for robot type '{id}'");
            var filename = Path.GetFileName(file.Name);
            this._logger.LogInformation($"Uploading '{filename}'");
            await RobotTypeFilePackage.StorePackageFile(robotType, this.rootFolder, filename, file.Value);

            return new ExecutionResult();
        }

        [HttpGet("{id}/package/{filename}")]
        [AllowAnonymous]
        public async Task<ActionResult> RetrievePackageFile(string id, string filename)
        {
            this._logger.LogDebug($"Retrieving robot type: {id}");
            var queryable = this.session.Query<RobotType>();
            var robotType = await queryable.FirstOrDefaultAsync(u => u.Name == id);
            if (robotType == null)
            {
                return NotFound();
            }

            string etag = string.Empty;
            if (Request.Headers.ContainsKey("ETag"))
            {
                etag = Request.Headers["ETag"].First();
                if (etag.StartsWith("\"", StringComparison.Ordinal) && etag.EndsWith("\"", StringComparison.Ordinal))
                {
                    etag = etag[1..^1];
                }
            }

            this._logger.LogInformation($"Retrieving file '{filename}' for robot type '{id}'");
            var details = await RobotTypeFilePackage.RetrieveFileAsync(robotType, this.rootFolder, filename, etag);
            if (details.StatusCode != HttpStatusCode.OK) return StatusCode((int)details.StatusCode);
            return File(details.DataStream, ContentTypes.Txt, filename);
        }

        [HttpPost("{id}/blocksets")]
        [Authorize(Policy = "Administrator")]
        public async Task<ActionResult<ExecutionResult>> AddBlockSet(string id, NamedValue? value)
        {
            if (value == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing blockset details"
                });
            }

            this._logger.LogInformation($"Adding new blockset for '{id}'");
            var command = new Commands.AddBlockSet
            {
                Name = value.Name,
                RobotType = id,
                Categories = value.Value
            };
            return await this.commandManager.ExecuteForHttp(command);
        }

        [HttpGet("{id}/blocksets")]
        public async Task<ActionResult<ListResult<NamedValue>>> GetBlockSets(string id)
        {
            this._logger.LogDebug($"Retrieving blocksets for {id}");
            var queryable = this.session.Query<RobotType>();
            var robotType = await queryable.FirstOrDefaultAsync(u => u.Name == id);
            if (robotType == null)
            {
                return NotFound();
            }

            this._logger.LogDebug($"Retrieved robot type ${robotType.Name}");
            var sets = robotType.BlockSets.Select(bs => new NamedValue { Name = bs.Name, Value = bs.BlockCategories }).AsEnumerable();
            return ListResult.New(sets);
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

        [HttpPost("{id}/toolbox")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult<ExecutionResult<Transfer.RobotType>>> ImportToolbox(string? id)
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
            return await this.commandManager.ExecuteForHttp(command, rt => Transfer.RobotType.FromModel(rt, Transfer.ConversionOptions.IncludeDetails));
        }
        */
    }
}