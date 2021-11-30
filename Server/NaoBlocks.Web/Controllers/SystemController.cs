using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Communications;
using NaoBlocks.Web.Helpers;
using System.Reflection;
using System.Text;

namespace NaoBlocks.Web.Controllers
{
    [Route("api/v1")]
    [ApiController]
    [Authorize]
    public class SystemController : ControllerBase
    {
        private readonly IExecutionEngine executionEngine;
        private readonly IHub communicationsHub;
        private readonly ILogger<SystemController> logger;

        public SystemController(ILogger<SystemController> logger, IExecutionEngine executionEngine, IHub hub)
        {
            this.logger = logger;
            this.executionEngine = executionEngine;
            this.communicationsHub = hub;
        }

        /// <summary>
        /// Attempts to initialise the system.
        /// </summary>
        /// <param name="administrator">The initial administrator settings.</param>
        /// <returns>A success <see cref="ExecutionResult"/> if the system was initialised, otherwise an error <see cref="ExecutionResult"/>.</returns>
        [HttpPost("system/initialise")]
        [AllowAnonymous]
        public async Task<ActionResult<ExecutionResult>> Initialise(Dtos.User administrator)
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
                }); ;
            }

            var command = new AddUser
            {
                Name = administrator.Name,
                Password = administrator.Password,
                Role = UserRole.Administrator
            };

            return await this.executionEngine
                .ExecuteForHttp(command)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Retreives the list of possible server addresses.
        /// </summary>
        /// <returns>A <see cref="ListResult{TData}"/> containing the server addresses.</returns>
        [HttpGet("system/addresses")]
        [AllowAnonymous]
        public Task<ActionResult<ListResult<string>>> ClientAddresses()
        {
            var addresses = new ListResult<string>
            {
                Items = ClientAddressList.Get()
            };
            addresses.Count = addresses.Items.Count();
            return Task.FromResult(new ActionResult<ListResult<string>>(addresses));
        }

        /// <summary>
        /// Retrieves the list of server addresses in a connection file.
        /// </summary>
        /// <returns>A <see cref="FileContentResult"/> containing the list of addresses.</returns>
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

        //[HttpGet("system/status")]
        //[Authorize("Administrator")]
        //public Task<ActionResult<Dtos.SystemStatus>> SystemStatus()
        //{
        //    var status = new Dtos.SystemStatus();
        //    foreach (var robot in this.communicationsHub.GetClients(ClientConnectionType.Robot))
        //    {
        //        status.RobotsConnected.Add(new Dtos.RobotStatus
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
        //        status.UsersConnected.Add(new Dtos.UserStatus
        //        {
        //            Id = user.Id,
        //            Name = user.User?.Name ?? string.Empty,
        //            Status = user.Status.Message
        //        });
        //    }

        //    return Task.FromResult(new ActionResult<Dtos.SystemStatus>(status));
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
                Version = Assembly.GetEntryAssembly()
                    !.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    !.InformationalVersion,
                Status = isInitialised ? "ready" : "pending"
            };
        }

        [HttpGet("system/config")]
        [AllowAnonymous]
        public async Task<ActionResult<Dtos.SiteConfiguration>> GetSiteConfiguration()
        {
            this.logger.LogInformation($"Retrieving site configuration");
            var settings = await this.executionEngine
                .Query<SystemData>()
                .RetrieveSystemValuesAsync();
            return new Dtos.SiteConfiguration
            {
                DefaultAddress = settings.DefaultAddress
            };
        }

        [HttpPost("system/siteAddress")]
        [Authorize("Administrator")]
        public async Task<ActionResult<ExecutionResult<Dtos.SiteConfiguration>>> SetDefaultAddress(Dtos.SiteConfiguration? config)
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
            return await this.executionEngine.ExecuteForHttp<SystemValues, Dtos.SiteConfiguration>(
                command, 
                result => new Dtos.SiteConfiguration { DefaultAddress = result?.DefaultAddress });
        }

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

        //[HttpGet("whoami")]
        //public async Task<ActionResult<Dtos.User>> WhoAmI()
        //{
        //    this.logger.LogInformation("Retrieving current user");
        //    var user = await this.LoadUser(this.session).ConfigureAwait(false);
        //    if (user == null) return this.NotFound();
        //    return new Dtos.User
        //    {
        //        Name = user.Name,
        //        Role = user.Role.ToString()
        //    };
        //}
    }
}
