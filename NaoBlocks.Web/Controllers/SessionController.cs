using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NaoBlocks.Core.Commands;
using NaoBlocks.Core.Models;
using NaoBlocks.Web.Helpers;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class SessionController : ControllerBase
    {
        private readonly ILogger<SessionController> _logger;
        private readonly ICommandManager commandManager;
        private readonly string jwtSecret;
        private readonly IAsyncDocumentSession session;

        public SessionController(ILogger<SessionController> logger, ICommandManager commandManager, IAsyncDocumentSession session, IOptions<AppSettings> appSettings)
        {
            this._logger = logger;
            this.commandManager = commandManager;
            this.session = session;
            this.jwtSecret = (appSettings?.Value ?? new AppSettings()).JwtSecret ?? "<Unknown Secret Key>";
        }

        public Func<DateTime> CurrentTimeFunc
        {
            get;
            set;
        } = () => DateTime.UtcNow;

        public async Task<ActionResult<Dtos.ExecutionResult>> Delete()
        {
            var user = await this.LoadUser(this.session).ConfigureAwait(false);
            if (user == null) return NotFound();

            var command = new FinishSessionCommand { UserId = user.Id };
            return await this.commandManager.ExecuteForHttp(command);
        }

        [HttpGet]
        public async Task<ActionResult<Dtos.UserSession>> Get()
        {
            var user = await this.LoadUser(this.session).ConfigureAwait(false);
            if (user == null) return NotFound();

            var now = this.CurrentTimeFunc();
            var session = await this.session.Query<Session>()
                .FirstOrDefaultAsync(s => s.UserId == user.Id && s.WhenExpires > now);
            var remaining = session != null
                ? session.WhenExpires.Subtract(now).TotalMinutes
                : -1;
            return new Dtos.UserSession
            {
                Name = user.Name,
                Role = user.Role.ToString(),
                TimeRemaining = Convert.ToInt32(remaining)
            };
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<Dtos.ExecutionResult<Dtos.Session>>> Post(Dtos.User? user)
        {
            if (user == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing session details"
                });
            }

            this._logger.LogInformation($"Starting new session for '{user.Name}'");
            var command = new StartSessionCommand
            {
                Password = user.Password,
                Name = user.Name
            };

            return await this.ApplyCommand(command).ConfigureAwait(false);
        }

        [HttpPut]
        public async Task<ActionResult<Dtos.ExecutionResult>> Put(Session value)
        {
            var user = await this.LoadUser(this.session).ConfigureAwait(false);
            if (user == null) return NotFound();

            var command = new RenewSessionCommand { UserId = user.Id };
            return await this.commandManager.ExecuteForHttp(command);
        }

        private async Task<ActionResult<Dtos.ExecutionResult<Dtos.Session>>> ApplyCommand(CommandBase<Session> command)
        {
            var errors = await commandManager.ValidateAsync(command).ConfigureAwait(false);
            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    this._logger.LogInformation(error);
                }

                return new BadRequestObjectResult(new Dtos.ExecutionResult<Dtos.Session>
                {
                    ValidationErrors = new[] { "Unable to validate session details" }
                });
            }

            var result = await commandManager.ApplyAsync(command).ConfigureAwait(false);
            if (!result.WasSuccessful)
            {
                return new ObjectResult(new Dtos.ExecutionResult<Dtos.Session>
                {
                    ExecutionErrors = new[] { result.Error ?? string.Empty }
                })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(this.jwtSecret);
            var expiry = command.Output?.WhenExpires ?? DateTime.UtcNow;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, command.Output?.UserId ?? string.Empty),
                    new Claim(ClaimTypes.Role, (command.Output?.Role ?? UserRole.Student).ToString()),
                    new Claim("SessionId", command.Output?.Id ?? string.Empty)
                }),
                Expires = expiry,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            await commandManager.CommitAsync().ConfigureAwait(false);
            return Dtos.ExecutionResult.New(new Dtos.Session
            {
                Token = tokenHandler.WriteToken(token),
                Expires = expiry,
                Role = (command?.Output?.Role.ToString() ?? "Unknown")
            });
        }
    }
}