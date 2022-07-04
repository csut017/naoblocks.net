using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Helpers;
using Newtonsoft.Json;
using Data = NaoBlocks.Engine.Data;
using Generators = NaoBlocks.Engine.Generators;
using Transfer = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Controllers
{
    /// <summary>
    /// A controller for working with robot types.
    /// </summary>
    [Route("api/v1/robots/types")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class RobotTypesController : ControllerBase
    {
        private readonly IExecutionEngine executionEngine;
        private readonly ILogger<RobotTypesController> logger;
        private readonly string rootFolder;

        /// <summary>
        /// Initialises a new <see cref="RobotTypesController"/> instance.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="executionEngine">The execution engine for processing commands and queries.</param>
        /// <param name="hostingEnvironment">The web hosting environment.</param>
        public RobotTypesController(ILogger<RobotTypesController> logger, IExecutionEngine executionEngine, IWebHostEnvironment hostingEnvironment)
        {
            this.logger = logger;
            this.executionEngine = executionEngine;
            this.rootFolder = hostingEnvironment.WebRootPath;
        }

        /// <summary>
        /// Adds a blockset to a robot type.
        /// </summary>
        /// <param name="id">The id of the robot type.</param>
        /// <param name="value">The details of the blockset.</param>
        /// <returns>The result of execution.</returns>
        [HttpPost("{id}/blocksets")]
        [Authorize(Policy = "Administrator")]
        public async Task<ActionResult<ExecutionResult>> AddBlockSet(string id, Data.NamedValue? value)
        {
            if (value == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing blockset details"
                });
            }

            this.logger.LogInformation($"Adding new blockset for '{id}'");
            var command = new AddBlockSet
            {
                Name = value.Name,
                RobotType = id,
                Categories = value.Value
            };
            return await this.executionEngine.ExecuteForHttp(command);
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
            this.logger.LogInformation($"Deleting robot type '{id}'");
            var command = new DeleteRobotType
            {
                Name = id
            };
            return await this.executionEngine.ExecuteForHttp(command);
        }

        /// <summary>
        /// Generates an export package for a robot type.
        /// </summary>
        /// <param name="id">The name of the robot type.</param>
        /// <param name="format">The export format.</param>
        /// <returns>The robot type export package.</returns>
        [HttpGet("export/package/{id}")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult> ExportPackage(string id, string? format)
        {
            return await this.GenerateReport<Generators.RobotTypePackage>(
                this.executionEngine,
                format,
                defaultFormat: ReportFormat.Zip);
        }

        /// <summary>
        /// Retrieves a robot type by its name.
        /// </summary>
        /// <param name="name">The name of the robot type.</param>
        /// <returns>Either a 404 (not found) or the robot type details.</returns>
        [HttpGet("{name}")]
        public async Task<ActionResult<Transfer.RobotType>> Get(string name)
        {
            this.logger.LogDebug($"Retrieving robot type: {name}");
            var robotType = await this.executionEngine
                .Query<RobotTypeData>()
                .RetrieveByNameAsync(name)
                .ConfigureAwait(false);
            if (robotType == null)
            {
                return NotFound();
            }

            this.logger.LogDebug($"Retrieved robot type ${robotType.Name}");
            return Transfer.RobotType.FromModel(robotType);
        }

        /// <summary>
        /// Imports a toolbox for a robot type.
        /// </summary>
        /// <param name="id">The name of the robot type.</param>
        /// <returns>The result of execution.</returns>
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

            this.logger.LogInformation($"Updating robot type '{id}'");
            var command = new ImportToolbox
            {
                Name = id,
                Definition = xml
            };
            return await this.executionEngine
                .ExecuteForHttp<Data.RobotType, Transfer.RobotType>
                (command,
                rt => Transfer.RobotType.FromModel(rt!, true));
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
            this.logger.LogDebug($"Retrieving robot types: page {pageNum} with size {pageSize}");
            var robotTypes = await this.executionEngine
                .Query<RobotTypeData>()
                .RetrievePageAsync(pageNum, pageSize)
                .ConfigureAwait(false);
            var count = robotTypes.Items?.Count() ?? 0;
            this.logger.LogDebug($"Retrieved {count} robot types");
            var result = new ListResult<Transfer.RobotType>
            {
                Count = robotTypes.Count,
                Page = pageNum,
                Items = robotTypes.Items?.Select(r => Transfer.RobotType.FromModel(r))
            };
            return result;
        }

        /// <summary>
        /// Retrieves a page of blocksets for a robot type.
        /// </summary>
        /// <param name="id">The name of the robot type.</param>
        /// <param name="include">The extra details to include. Options are ["blocks"].</param>
        /// <returns>A <see cref="ListResult{TData}"/> containing the blocksets.</returns>
        [HttpGet("{id}/blocksets")]
        public async Task<ActionResult<Transfer.RobotBlockDefinitions>> ListBlockSets(string id, [FromQuery] params string[] include)
        {
            this.logger.LogDebug($"Retrieving blocksets for {id}");
            var robotType = await this.executionEngine
                .Query<RobotTypeData>()
                .RetrieveByNameAsync(id)
                .ConfigureAwait(false);
            if (robotType == null)
            {
                return NotFound();
            }

            this.logger.LogDebug($"Retrieved robot type ${robotType.Name}");
            var sets = robotType.BlockSets
                .Select(bs => new Data.NamedValue { Name = bs.Name, Value = bs.BlockCategories })
                .AsEnumerable();
            var response = new Transfer.RobotBlockDefinitions
            {
                BlockSets = sets
            };

            // Include the blocks if requested
            if (include.Any(p => p.Equals("blocks", StringComparison.InvariantCultureIgnoreCase)))
            {
                var blocks = new Dictionary<string, Transfer.BlockDefinition>();
                foreach (var category in robotType.Toolbox)
                {
                    foreach (var block in category.Blocks)
                    {
                        if (!blocks.TryGetValue(block.Name, out var blockDefinition))
                        {
                            blockDefinition = new Transfer.BlockDefinition
                            {
                                Name = block.Name
                            };
                            blocks.Add(block.Name, blockDefinition);
                        }
                        blockDefinition.AddCategory(category.Name);
                    }
                }

                response.Blocks = blocks.Values
                    .OrderBy(b => b.Name)
                    .ToArray();
            }

            return response;
        }

        /// <summary>
        /// Adds a new robot type.
        /// </summary>
        /// <param name="robotType">The robot type to add.</param>
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

            this.logger.LogInformation($"Adding new robot type '{robotType.Name}'");
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
        /// <param name="robotType">The details of the robot type.</param>
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

            this.logger.LogInformation($"Updating robot type '{id}'");
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

        /// <summary>
        /// Sets a robot type as the system default.
        /// </summary>
        /// <param name="id">The name of the robot type.</param>
        /// <returns>The result of execution.</returns>
        [HttpPut("{id}/default")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult<ExecutionResult<Transfer.RobotType>>> SetAsDefault(string? id)
        {
            this.logger.LogInformation($"Updating robot type '{id}'");
            var command = new SetDefaultRobotType
            {
                Name = id
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
        */
    }
}