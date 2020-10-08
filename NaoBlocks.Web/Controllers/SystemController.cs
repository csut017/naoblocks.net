using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NaoBlocks.Core.Models;
using NaoBlocks.Web.Communications;
using NaoBlocks.Web.Helpers;
using QRCoder;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Commands = NaoBlocks.Core.Commands;

namespace NaoBlocks.Web.Controllers
{
    [Route("api/v1")]
    [ApiController]
    [Authorize]
    [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
    public class SystemController : ControllerBase
    {
        private readonly Commands.ICommandManager _commandManager;
        private readonly IHub _hub;
        private readonly ILogger<SystemController> _logger;
        private readonly IAsyncDocumentSession _session;

        public SystemController(ILogger<SystemController> logger, Commands.ICommandManager commandManager, IAsyncDocumentSession session, IHub hub)
        {
            this._logger = logger;
            this._commandManager = commandManager;
            this._session = session;
            this._hub = hub;
        }

        [HttpPost("system/initialise")]
        [AllowAnonymous]
        public async Task<ActionResult<Dtos.ExecutionResult>> Initialise(Dtos.User? administrator)
        {
            if (administrator is null) throw new ArgumentNullException(nameof(administrator));

            this._logger.LogWarning("Initialising system");

            var hasUsers = await this._session.Query<User>()
                .AnyAsync()
                .ConfigureAwait(false);
            if (hasUsers)
            {
                return new BadRequestObjectResult(new Dtos.ExecutionResult
                {
                    ValidationErrors = new[] {
                        new Commands.CommandError(0, "System already initialised")
                    }
                }); ;
            }

            var command = new Commands.AddUser
            {
                Name = administrator.Name,
                Password = administrator.Password,
                Role = UserRole.Administrator
            };

            return await this._commandManager.ExecuteForHttp(command).ConfigureAwait(false);
        }

        [HttpGet("system/addresses")]
        [AllowAnonymous]
        public Task<ActionResult<Dtos.ListResult<string>>> ClientAddresses()
        {
            var addresses = new Dtos.ListResult<string>
            {
                Items = ClientAddressList.Get()
            };
            addresses.Count = addresses.Items.Count();
            return Task.FromResult(new ActionResult<Dtos.ListResult<string>>(addresses));
        }

        [HttpGet("system/addresses/connect.txt")]
        [AllowAnonymous]
        public Task<ActionResult> ClientAddressesFile()
        {
            var addresses = ClientAddressList.Get();
            var data = string.Join(
                '\n', 
                addresses.Select(a => a + ",," + (a.StartsWith("https:", StringComparison.OrdinalIgnoreCase) ? "yes" : "no")));
            var file = new FileContentResult(Encoding.UTF8.GetBytes(data), ContentTypes.Txt)
            {
                FileDownloadName = "connect.txt"
            };
            return Task.FromResult((ActionResult)file);
        }

        [HttpGet("system/status")]
        [Authorize("Administrator")]
        public Task<ActionResult<Dtos.SystemStatus>> SystemStatus()
        {
            var status = new Dtos.SystemStatus();
            foreach (var robot in this._hub.GetClients(ClientConnectionType.Robot))
            {
                status.RobotsConnected.Add(new Dtos.RobotStatus
                {
                    Id = robot.Id,
                    MachineName = robot.Robot?.MachineName ?? string.Empty,
                    FriendlyName = robot.Robot?.FriendlyName ?? string.Empty,
                    IsAvailable = robot.Status.IsAvailable,
                    Status = robot.Status.Message
                });
            }

            foreach (var user in this._hub.GetClients(ClientConnectionType.User))
            {
                status.UsersConnected.Add(new Dtos.UserStatus
                {
                    Id = user.Id,
                    Name = user.User?.Name ?? string.Empty,
                    Status = user.Status.Message
                });
            }

            return Task.FromResult(new ActionResult<Dtos.SystemStatus>(status));
        }

        [HttpGet("version")]
        [AllowAnonymous]
        public ActionResult<object> Version()
        {
            this._logger.LogInformation("Retrieving system version number");
            return new
            {
                Version = Assembly.GetEntryAssembly()
                    ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion
            };
        }

        [HttpGet("system/config")]
        [AllowAnonymous]
        public async Task<ActionResult<Dtos.SiteConfiguration>> GetSiteConfiguration()
        {
            this._logger.LogInformation($"retrieving site configuration");
            var settings = await this._session.Query<SystemValues>()
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
            return new Dtos.SiteConfiguration
            {
                DefaultAddress = settings?.DefaultAddress
            };
        }

        [HttpPost("system/siteAddress")]
        [Authorize("Administrator")]
        public async Task<ActionResult<Dtos.ExecutionResult<Dtos.SiteConfiguration>>> SetDefaultAddress(Dtos.SiteConfiguration config)
        {
            if (config == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing configuration settings"
                });
            }

            this._logger.LogInformation($"Updating default site address to '{config.DefaultAddress}'");
            var command = new Commands.StoreDefaultAddress
            {
                Address = config.DefaultAddress
            };
            return await this._commandManager.ExecuteForHttp(command, result => new Dtos.SiteConfiguration { DefaultAddress = result?.DefaultAddress });
        }

        [HttpGet("system/qrcode")]
        [HttpGet("system/qrcode/{address}")]
        [AllowAnonymous]
        public async Task<ActionResult> GenerateQRCode(string? address)
        {
            if (string.IsNullOrEmpty(address))
            {
                var config = await this._session.Query<SystemValues>()
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);
                address = config?.DefaultAddress ?? string.Empty;
            }

            var decodedAddress = HttpUtility.UrlDecode(address);
            this._logger.LogInformation("Generating QR code");
            using var generator = new QRCodeGenerator();
            var codeData = generator.CreateQrCode(decodedAddress, QRCodeGenerator.ECCLevel.Q);
            using var code = new QRCode(codeData);
            var image = code.GetGraphic(20);
            using var stream = new MemoryStream();
            image.Save(stream, ImageFormat.Png);
            return File(stream.ToArray(), ContentTypes.Png);
        }

        [HttpGet("whoami")]
        public async Task<ActionResult<Dtos.User>> WhoAmI()
        {
            this._logger.LogInformation("Retrieving current user");
            var user = await this.LoadUser(this._session).ConfigureAwait(false);
            if (user == null) return this.NotFound();
            return new Dtos.User
            {
                Name = user.Name,
                Role = user.Role.ToString()
            };
        }
    }
}