﻿using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Communications;
using NaoBlocks.Web.Helpers;
using Data = NaoBlocks.Engine.Data;
using Generators = NaoBlocks.Engine.Generators;
using Transfer = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Controllers
{
    /// <summary>
    /// Controller for system-level actions.
    /// </summary>
    [Route("api/v1")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class SystemController : ControllerBase
    {
        private readonly IHub communicationsHub;
        private readonly IExecutionEngine executionEngine;
        private readonly ILogger<SystemController> logger;
        private readonly IServer server;
        private readonly UiManager uiManager;

        /// <summary>
        /// Initialise a new <see cref="SystemController"/> instance.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="executionEngine">The execution engine for processing commands and queries.</param>
        /// <param name="hub">The client communications hub.</param>
        /// <param name="uiManager">The <see cref="UiManager"/> to use.</param>
        /// <param name="server">The hosting server.</param>
        public SystemController(ILogger<SystemController> logger, IExecutionEngine executionEngine, IHub hub, UiManager uiManager, IServer server)
        {
            this.logger = logger;
            this.executionEngine = executionEngine;
            this.communicationsHub = hub;
            this.uiManager = uiManager;
            this.server = server;
        }

        /// <summary>
        /// Retreives the list of possible server addresses.
        /// </summary>
        /// <returns>A <see cref="ListResult{TData}"/> containing the server addresses.</returns>
        [HttpGet("system/addresses")]
        [AllowAnonymous]
        public Task<ActionResult<ListResult<string>>> ClientAddresses()
        {
            ICollection<string> addresses = this.RetrieveServerAddresses();
            var result = ListResult.New(addresses);
            return Task.FromResult(new ActionResult<ListResult<string>>(result));
        }

        /// <summary>
        /// Retrieves the list of server addresses in a connection file.
        /// </summary>
        /// <returns>A <see cref="FileContentResult"/> containing the list of addresses.</returns>
        [HttpGet("system/addresses/connect.txt")]
        [AllowAnonymous]
        public Task<ActionResult> ClientAddressesFile()
        {
            var addresses = this.RetrieveServerAddresses();
            var data = string.Join(
                '\n',
                addresses.Select(a => a + ",," + (a.StartsWith("https:", StringComparison.OrdinalIgnoreCase) ? "yes" : "no")));
            var file = new FileContentResult(Encoding.UTF8.GetBytes(data), ContentTypes.FromReportFormat(ReportFormat.Text))
            {
                FileDownloadName = "connect.txt"
            };
            return Task.FromResult((ActionResult)file);
        }

        /// <summary>
        /// Exports all the configuration in the system.
        /// </summary>
        /// <param name="format">The export format.</param>
        /// <returns>The configuration settings.</returns>
        [HttpGet("system/export/allConfig")]
        [HttpGet("system/export/allConfig{format}")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult> ExportAllConfiguration(string? format = ".xlsx")
        {
            var args = new Dictionary<string, string>
            {
                { "robots", "yes" },
                { "students", "yes" },
            };
            var user = await this.LoadUserAsync(this.executionEngine)
                .ConfigureAwait(false);
            if (user?.Role == Data.UserRole.Administrator)
            {
                args.Add("types", "yes");
            }

            return await this.GenerateReport<Generators.AllLists>(
                this.executionEngine,
                format,
                defaultFormat: ReportFormat.Csv,
                args: args);
        }

        /// <summary>
        /// Exports all the code programs in the system.
        /// </summary>
        /// <param name="format">The export format.</param>
        /// <param name="from">The start date.</param>
        /// <param name="to">The end date.</param>
        /// <returns>The robot logs.</returns>
        [HttpGet("system/export/programs")]
        [HttpGet("system/export/programs{format}")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult> ExportCodePrograms(string? format = ".xlsx", string? from = null, string? to = null)
        {
            var args = this.MakeArgs($"from={from}", $"to={to}");
            return await this.GenerateReport<Generators.CodePrograms>(
                this.executionEngine,
                format,
                defaultFormat: ReportFormat.Csv,
                args: args);
        }

        /// <summary>
        /// Exports all the robot logs in the system.
        /// </summary>
        /// <param name="format">The export format.</param>
        /// <param name="from">The start date.</param>
        /// <param name="to">The end date.</param>
        /// <returns>The robot logs.</returns>
        [HttpGet("system/export/logs")]
        [HttpGet("system/export/logs{format}")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult> ExportRobotLogs(string? format = ".xlsx", string? from = null, string? to = null)
        {
            var args = this.MakeArgs($"from={from}", $"to={to}");
            return await this.GenerateReport<Generators.RobotLogs>(
                this.executionEngine,
                format,
                defaultFormat: ReportFormat.Csv,
                args: args);
        }

        /// <summary>
        /// Retrieves the site configuration options.
        /// </summary>
        /// <returns>A <see cref="Transfer.SiteConfiguration"/> containing the configuration details.</returns>
        [HttpGet("system/config")]
        [AllowAnonymous]
        public async Task<ActionResult<Transfer.SiteConfiguration>> GetSiteConfiguration()
        {
            this.logger.LogInformation($"Retrieving site configuration");
            var settings = await this.executionEngine
                .Query<SystemData>()
                .RetrieveSystemValuesAsync();
            return new Transfer.SiteConfiguration
            {
                DefaultAddress = settings.DefaultAddress
            };
        }

        /// <summary>
        /// Attempts to initialise the system.
        /// </summary>
        /// <param name="settings">The initial settings.</param>
        /// <returns>A success <see cref="ExecutionResult"/> if the system was initialised, otherwise an error <see cref="ExecutionResult"/>.</returns>
        [HttpPost("system/initialise")]
        [AllowAnonymous]
        public async Task<ActionResult<ExecutionResult>> Initialise(Transfer.InitialisationSettings settings)
        {
            this.logger.LogWarning("Initialising system");

            var hasUsers = await this.executionEngine
                .Query<UserData>()
                .CheckForAnyAsync();
            if (hasUsers)
            {
                return new BadRequestObjectResult(new ExecutionResult
                {
                    ValidationErrors = new[] {
                        new CommandError(0, "System already initialised")
                    }
                });
            }

            CommandBase command = new AddUser
            {
                Name = "admin",
                Password = settings.Password,
                Role = Data.UserRole.Administrator
            };
            var useBatch = false;
            var commands = new List<CommandBase>
                {
                    command
                };
            if (settings.UseDefaultUi)
            {
                foreach (var ui in this.uiManager.ListRegistered())
                {
                    var template = this.uiManager.ReadTemplate(ui.Key);
                    commands.Add(new AddUIDefinition
                    {
                        Definition = this.uiManager.Parse(ui.Key, template),
                        Name = ui.Key,
                        IgnoreExisting = true
                    });
                }
                useBatch = true;
            }

            if (settings.AddNaoRobot)
            {
                commands.Add(new AddRobotType { Name = "Nao" });
                commands.Add(new SetDefaultRobotType { Name = "Nao", IgnoreMissingRobotType = true });
                commands.Add(new ImportToolbox
                {
                    RobotTypeName = "Nao",
                    Definition = Resources.Manager.NaoToolbox,
                    ToolboxName = "Default",
                    IsDefault = true,
                    IgnoreMissingRobotType = true
                });
                useBatch = true;
            }

            if (useBatch)
            {
                command = new Batch(commands.ToArray());
            }

            return await this.executionEngine
                .ExecuteForHttp(command)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the default site address.
        /// </summary>
        /// <param name="config">The configuration options containing the site address.</param>
        /// <returns>The result of the command execution.</returns>
        [HttpPost("system/siteAddress")]
        [Authorize("Administrator")]
        public async Task<ActionResult<ExecutionResult<Transfer.SiteConfiguration>>> SetDefaultAddress(Transfer.SiteConfiguration? config)
        {
            if (config == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing configuration settings"
                });
            }

            this.logger.LogInformation($"Updating default site address to '{config.DefaultAddress}'");
            var command = new StoreDefaultAddress
            {
                Address = config.DefaultAddress
            };
            return await this.executionEngine.ExecuteForHttp<Data.SystemValues, Transfer.SiteConfiguration>(
                command,
                result => new Transfer.SiteConfiguration { DefaultAddress = result?.DefaultAddress });
        }

        //    return Task.FromResult(new ActionResult<Transfer.SystemStatus>(status));
        //}
        /// <summary>
        /// Retrieves the current API version and status.
        /// </summary>
        /// <returns>A <see cref="VersionInformation"/> instance.</returns>
        [HttpGet("version")]
        [AllowAnonymous]
        public async Task<ActionResult<VersionInformation>> Version()
        {
            var isInitialised = await this.executionEngine
                .Query<UserData>()
                .CheckForAnyAsync()
                .ConfigureAwait(false);

            this.logger.LogInformation("Retrieving system version number");
            return new VersionInformation
            {
                Version = ControllerHelpers.GetVersion(),
                Status = isInitialised ? "ready" : "pending"
            };
        }

        /// <summary>
        /// Retrieves the details of the current user.
        /// </summary>
        /// <returns>The current user details, if authenticated, not found otherwise.</returns>
        [HttpGet("whoami")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Transfer.User>> WhoAmI()
        {
            this.logger.LogInformation("Retrieving current user");
            var user = await this.LoadUserAsync(this.executionEngine)
                .ConfigureAwait(false);
            if (user == null) return this.NotFound();
            return new Transfer.User
            {
                Name = user.Name,
                Role = user.Role.ToString()
            };
        }

        private ICollection<string> RetrieveServerAddresses()
        {
            var addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses ?? Array.Empty<string>();
            if (!addresses.Any())
            {
                return new[]
                {
                    "https://127.0.0.1"
                };
            }

            return addresses;
        }

        //[HttpGet("system/status")]
        //[Authorize("Administrator")]
        //public Task<ActionResult<Transfer.SystemStatus>> SystemStatus()
        //{
        //    var status = new Transfer.SystemStatus();
        //    foreach (var robot in this.communicationsHub.GetClients(ClientConnectionType.Robot))
        //    {
        //        status.RobotsConnected.Add(new Transfer.RobotStatus
        //        {
        //            Id = robot.Id,
        //            MachineName = robot.Robot?.MachineName ?? string.Empty,
        //            FriendlyName = robot.Robot?.FriendlyName ?? string.Empty,
        //            IsAvailable = robot.Status.IsAvailable,
        //            Status = robot.Status.Message
        //        });
        //    }

        //    foreach (var user in this.communicationsHub.GetClients(ClientConnectionType.User))
        //    {
        //        status.UsersConnected.Add(new Transfer.UserStatus
        //        {
        //            Id = user.Id,
        //            Name = user.User?.Name ?? string.Empty,
        //            Status = user.Status.Message
        //        });
        //    }
        //[HttpGet("system/qrcode")]
        //[HttpGet("system/qrcode/{address}")]
        //[AllowAnonymous]
        //public async Task<ActionResult> GenerateQRCode(string? address)
        //{
        //    if (string.IsNullOrEmpty(address))
        //    {
        //        var config = await this.session.Query<SystemValues>()
        //            .FirstOrDefaultAsync()
        //            .ConfigureAwait(false);
        //        address = config?.DefaultAddress ?? string.Empty;
        //    }

        //    var decodedAddress = HttpUtility.UrlDecode(address);
        //    this.logger.LogInformation("Generating QR code");
        //    using var generator = new QRCodeGenerator();
        //    var codeData = generator.CreateQrCode(decodedAddress, QRCodeGenerator.ECCLevel.Q);
        //    using var code = new QRCode(codeData);
        //    var image = code.GetGraphic(20);
        //    using var stream = new MemoryStream();
        //    image.Save(stream, ImageFormat.Png);
        //    return File(stream.ToArray(), ContentTypes.Png);
        //}
    }
}