using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NaoBlocks.Core.Commands;
using NaoBlocks.Core.Models;
using NaoBlocks.Web.Helpers;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Controllers
{
    [Route("api/v1")]
    [ApiController]
    [Authorize]
    [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
    public class SystemController : ControllerBase
    {
        private readonly ICommandManager commandManager;
        private readonly ILogger<SystemController> logger;
        private readonly IAsyncDocumentSession session;

        public SystemController(ILogger<SystemController> logger, ICommandManager commandManager, IAsyncDocumentSession session)
        {
            this.logger = logger;
            this.commandManager = commandManager;
            this.session = session;
        }

        [HttpPost("system/initialise")]
        [AllowAnonymous]
        public async Task<ActionResult<Dtos.ExecutionResult>> Initialise(Dtos.User? administrator)
        {
            if (administrator is null) throw new ArgumentNullException(nameof(administrator));

            this.logger.LogWarning("Initialising system");

            var hasUsers = await this.session.Query<User>()
                .AnyAsync()
                .ConfigureAwait(false);
            if (hasUsers)
            {
                return new BadRequestObjectResult(new Dtos.ExecutionResult
                {
                    ValidationErrors = new[] { "System already initialised" }
                }); ;
            }

            var command = new AddUserCommand
            {
                Name = administrator.Name,
                Password = administrator.Password,
                Role = UserRole.Administrator
            };

            return await this.commandManager.ExecuteForHttp(command).ConfigureAwait(false);
        }

        [HttpGet("version")]
        [AllowAnonymous]
        public ActionResult<object> Version()
        {
            this.logger.LogInformation("Retrieving system version number");
            return new
            {
                Version = Assembly.GetEntryAssembly()
                    ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion
            };
        }

        //[HttpGet("whoami")]
        //public async Task<ActionResult<Dtos.User>> WhoAmI()
        //{
        //    this.logger.LogInformation("Retrieving current user");
        //    var user = await this.LoadUser(this.session).ConfigureAwait(false);
        //    if (user == null) return this.NotFound();
        //    return new Dtos.User
        //    {
        //        Name = user.Name,
        //        Login = user.Login,
        //        Type = user.Type.ToString()
        //    };
        //}
    }
}