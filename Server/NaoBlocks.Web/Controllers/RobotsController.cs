﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NaoBlocks.Common;
using NaoBlocks.Communications;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Configuration;
using NaoBlocks.Web.Dtos;
using NaoBlocks.Web.Helpers;
using QRCoder;
using System.Web;

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
        private readonly IOptions<Addresses> configuration;
        private readonly IExecutionEngine executionEngine;
        private readonly ILogger<RobotsController> logger;

        /// <summary>
        /// Initialises a new <see cref="RobotsController"/> instance.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="engine">The execution engine for processing commands and queries.</param>
        /// <param name="configuration">The address configuration.</param>
        public RobotsController(ILogger<RobotsController> logger, IExecutionEngine engine, IOptions<Addresses> configuration)
        {
            this.logger = logger;
            this.executionEngine = engine;
            this.configuration = configuration;
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
            this.logger.LogInformation("Deleting robot '{id}'", id);
            var command = new DeleteRobot
            {
                Name = id
            };
            return await this.executionEngine.ExecuteForHttp(command);
        }

        /// <summary>
        /// Generates the robot details export.
        /// </summary>
        /// <param name="name">The name of the robot.</param>
        /// <param name="format">The format to use.</param>
        /// <returns>The generated robot details.</returns>
        /// <param name="from">The start date.</param>
        /// <param name="to">The end date.</param>
        [HttpGet("{name}/export")]
        [HttpGet("{name}/export.{format}")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult> ExportDetails(string name, string? format = "xlsx", string? from = null, string? to = null)
        {
            this.logger.LogInformation("Generating robot details export");
            var args = this.MakeArgs($"from={from}", $"to={to}");
            return await this.GenerateRobotReport<Generators.RobotExport>(
                this.executionEngine,
                format,
                name,
                defaultFormat: ReportFormat.Excel,
                args: args);
        }

        /// <summary>
        /// Generates the robot list export.
        /// </summary>
        /// <param name="format">The format to use.</param>
        /// <param name="flags">The optional export flags.</param>
        /// <returns>The generated robot list.</returns>
        [HttpGet("export")]
        [HttpGet("export.{format}")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult> ExportList(string? format, [FromQuery] string? flags = null)
        {
            this.logger.LogInformation("Generating robot list export");
            return await this.GenerateReport<Generators.RobotsList>(
                this.executionEngine,
                format,
                args: this.ParseFlags(flags));
        }

        /// <summary>
        /// Exports the logs for a robot.
        /// </summary>
        /// <param name="name">The name of the robot.</param>
        /// <param name="format">The report format to generate.</param>
        /// <returns>The generated robot logs.</returns>
        /// <param name="from">The start date.</param>
        /// <param name="to">The end date.</param>
        [HttpGet("{name}/logs/export")]
        [HttpGet("{name}/logs/export.{format}")]
        public async Task<ActionResult> ExportLogs(string? name, string? format = "xlsx", string? from = null, string? to = null)
        {
            this.logger.LogInformation("Generating robot log export");
            var args = this.MakeArgs($"from={from}", $"to={to}");
            return await this.GenerateRobotReport<Generators.RobotLogs>(
                this.executionEngine,
                format,
                name,
                defaultFormat: ReportFormat.Excel,
                args: args);
        }

        /// <summary>
        /// Retrieves a robot by its machine name.
        /// </summary>
        /// <param name="name">The name of the robot.</param>
        /// <param name="includeType">A flag indicating whether the type details should be included or not.</param>
        /// <returns>Either a 404 (not found) or the robot details.</returns>
        [HttpGet("{name}")]
        public async Task<ActionResult<Transfer.Robot>> Get(string name, bool includeType = false)
        {
            this.logger.LogDebug("Retrieving robot: id {name}", name);
            var robot = await this.executionEngine
                .Query<RobotData>()
                .RetrieveByNameAsync(name, true)
                .ConfigureAwait(false);
            if (robot == null)
            {
                return NotFound();
            }

            this.logger.LogDebug("Retrieved robot");
            var dto = Transfer.Robot.FromModel(robot, DetailsType.Standard);

            if (includeType)
            {
                var robotType = await this.executionEngine
                    .Query<RobotTypeData>()
                    .RetrieveByIdAsync(robot.RobotTypeId)
                    .ConfigureAwait(false);

                if (robotType == null)
                {
                    // We should never get here, but it is possible to manually delete a robot type without
                    // deleting the actual robots
                    return NotFound();
                }
                this.logger.LogDebug("Retrieved robot type");
                dto.TypeDetails = Transfer.RobotType.FromModel(robotType, DetailsType.Standard);
            }

            return dto;
        }

        /// <summary>
        /// Generates a quick link for a robot.
        /// </summary>
        /// <param name="name">The name of the robot.</param>
        /// <param name="type">The type of quick link to generate.</param>
        /// <returns>Either a 404 (not found) or the quick link details.</returns>
        [HttpGet("{name}/quicklink/{type}")]
        [AllowAnonymous]
        public ActionResult<object> GetQuickLink(string name, string type)
        {
            if (string.IsNullOrEmpty(type)) return BadRequest(new
            {
                error = "Type of quick link was not specified"
            });

            var fullAddress = GenerateQuickLinkUrl(name, type);
            if (fullAddress == null) return BadRequest(new
            {
                error = $"Quick link {type} is not recognised"
            });
            this.logger.LogInformation("Quick link URL is {url}", fullAddress);

            return new
            {
                link = fullAddress,
            };
        }

        /// <summary>
        /// Generates a quick link QR code for a robot.
        /// </summary>
        /// <param name="name">The name of the robot.</param>
        /// <param name="type">The type of quick link to generate.</param>
        /// <param name="format">The format to generate.</param>
        /// <returns>Either a 404 (not found) or an image containing the QR code.</returns>
        [HttpGet("{name}/qrcode/{type}")]
        [HttpGet("{name}/qrcode/{type}.{format}")]
        [AllowAnonymous]
        public ActionResult GetQuickLinkQRCode(string name, string type, string? format = "png")
        {
            if (string.IsNullOrEmpty(type)) return BadRequest(new
            {
                error = "Type of quick link was not specified"
            });

            var fullAddress = GenerateQuickLinkUrl(name, type);
            if (fullAddress == null) return BadRequest(new
            {
                error = $"Quick link {type} is not recognised"
            });
            this.logger.LogInformation("Quick link URL is {url}", fullAddress);

            this.logger.LogDebug("Generating QR code for quick link {url}", fullAddress);
            using var generator = new QRCodeGenerator();
            var codeData = generator.CreateQrCode(fullAddress, QRCodeGenerator.ECCLevel.Q);
            using var code = new PngByteQRCode(codeData);
            var image = code.GetGraphic(20);
            return File(image, ContentTypes.Png, $"{name}.{format}");
        }

        /// <summary>
        /// Imports a list of robots.
        /// </summary>
        /// <param name="action">The action to perform. Options are "parse".</param>
        /// <returns>The result of execution.</returns>
        [HttpPost("import")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult<ExecutionResult<ListResult<Transfer.Robot>>>> Import([FromQuery] string? action)
        {
            if (!"parse".Equals(action))
            {
                return this.BadRequest(new
                {
                    Error = $"Action {action} is invalid"
                });
            }

            var files = this.Request?.Form?.Files;
            if (files == null)
            {
                return this.BadRequest(new
                {
                    Error = "Expected a file to process"
                });
            }

            if (files.Count != 1)
            {
                return this.BadRequest(new
                {
                    Error = "Invalid number of files, can only handle one file at a time"
                });
            }

            this.logger.LogInformation("Parsing robots import");
            using var inputStream = files[0].OpenReadStream();
            var command = new ParseRobotsImport
            {
                Data = inputStream
            };

            return await this.executionEngine
                .ExecuteForHttp<IEnumerable<Data.ItemImport<Data.Robot>>, ListResult<Transfer.Robot>>
                (command,
                robots => ListResult.New(robots.Select(r => Transfer.Robot.FromModel(r!, DetailsType.Parse))));
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
            this.logger.LogDebug("Retrieving robots: page {pageNum} with size {pageSize}", pageNum, pageSize);
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
            this.logger.LogDebug("Retrieved {count} robots", count);
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

            this.logger.LogInformation("Adding new robot '{robot}'", robot.MachineName);
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
        /// Updates the values on a robot.
        /// </summary>
        /// <param name="id">The id of the robot.</param>
        /// <param name="values">The values to update.</param>
        /// <returns>The result of execution.</returns>
        [HttpPost("{id}/values")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult<ExecutionResult>> PostValues(string id, Set<NamedValue> values)
        {
            var robot = await this.executionEngine
                .Query<RobotData>()
                .RetrieveByNameAsync(id)
                .ConfigureAwait(false);
            if (robot == null) return NotFound();

            this.logger.LogInformation("Updating values for robot '{robot}'", robot?.MachineName);
            var command = new UpdateCustomValuesForRobot
            {
                MachineName = id,
                Values = values?.Items ?? Array.Empty<NamedValue>()
            };
            return await this.executionEngine
                .ExecuteForHttp(command);
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

            this.logger.LogInformation("Updating robot '{robot}'", id);
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

            this.logger.LogInformation("Registering new robot '{robot}'", robot.MachineName);
            var command = new RegisterRobot
            {
                MachineName = robot.MachineName
            };
            return await this.executionEngine
                .ExecuteForHttp<Data.Robot, Transfer.Robot>
                (command, r => Transfer.Robot.FromModel(r!));
        }

        /// <summary>
        /// Generates a quick link URL.
        /// </summary>
        /// <param name="name">The name of the robot.</param>
        /// <param name="type">The type of link to generate.</param>
        /// <returns>A string containing the URL.</returns>
        private string? GenerateQuickLinkUrl(string name, string type)
        {
            var address = ClientAddressList.RetrieveAddresses().First();
            var typeAddress = type switch
            {
                "mobileview" => "mobile/robot",
                "editor" => "administrator/robots",
                _ => null,
            };
            return typeAddress == null
                ? null
                : $"https://{address}:{this.configuration.Value.HttpsPort}/{typeAddress}/{HttpUtility.UrlEncode(name)}";
        }
    }
}