﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Helpers;

namespace NaoBlocks.Web.Controllers
{
    /// <summary>
    /// A controller for working with sessions.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class SessionController : ControllerBase
    {
        private readonly ILogger<SessionController> _logger;
        private readonly IExecutionEngine executionEngine;
        private readonly string jwtSecret;

        /// <summary>
        /// Initialises a new <see cref="UsersController"/> instance.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="executionEngine">The execution engine for processing commands and queries.</param>
        /// <param name="configuration">The security configuration.</param>
        public SessionController(ILogger<SessionController> logger, IExecutionEngine executionEngine, IOptions<Configuration.Security> configuration)
        {
            this._logger = logger;
            this.executionEngine = executionEngine;
            this.jwtSecret = configuration.Value.JwtSecret ?? "<Unknown Secret Key>";
        }

        /// <summary>
        /// Gets or sets the function for retrieving the current time.
        /// </summary>
        /// <remarks>
        /// This property if mainly for enabling unit testing. Setting this property allows for
        /// setting the constant time.
        /// </remarks>
        public Func<DateTime> CurrentTimeFunc
        {
            get;
            set;
        } = () => DateTime.UtcNow;

        /// <summary>
        /// Deletes the current user's session.
        /// </summary>
        /// <returns>The result of execution.</returns>
        /// <response code="200">Returns the status of the command.</response>
        /// <response code="401">If the the request is not authenticated.</response>
        /// <response code="404">If the current user cannot be found.</response>
        [HttpDelete]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ExecutionResult>> Delete()
        {
            var user = await this.LoadUserAsync(this.executionEngine)
                .ConfigureAwait(false);
            if (user == null) return NotFound();

            var command = new FinishSession { UserName = user.Name };
            return await this.executionEngine.ExecuteForHttp(command);
        }

        /// <summary>
        /// Retrieves the details of the current user.
        /// </summary>
        /// <returns>A <see cref="UserSessionResult"/> containing the session details.</returns>
        /// <response code="200">Returns the current status information.</response>
        /// <response code="401">If the the request is not authenticated.</response>
        /// <response code="404">If the current user cannot be found.</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserSessionResult>> Get()
        {
            var user = await this.LoadUserAsync(this.executionEngine)
                .ConfigureAwait(false);
            if (user == null) return NotFound();

            var now = this.CurrentTimeFunc();
            var session = await this.executionEngine
                .Query<SessionData>()
                .RetrieveForUserAsync(user)
                .ConfigureAwait(false);
            var remaining = session != null
                ? session.WhenExpires.Subtract(now).TotalMinutes
                : -1;
            return new UserSessionResult
            {
                Name = user.Name,
                Role = user.Role.ToString(),
                TimeRemaining = Convert.ToInt32(remaining < 0 ? 0 : remaining)
            };
        }

        /*
        [HttpGet("settings")]
        public async Task<ActionResult<Transfer.EditorSettings>> GetSettings()
        {
            var user = await this.LoadUser(this.session).ConfigureAwait(false);
            if (user == null) return NotFound();

            var settings = await this.PrepareSettings(user);
            return settings;
        }

        private async Task<Transfer.EditorSettings> PrepareSettings(User? user, UserSettings? settings = null)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            var robotType = await this.RetrieveRobotTypeForUser(user);
            if (robotType != null)
            {
                var toolbox = Generators.UserToolbox.Generate(user, robotType);
                return new Transfer.EditorSettings
                {
                    CanConfigure = string.IsNullOrEmpty((settings ?? user.Settings).CustomBlockSet),
                    IsSystemInitialised = true,
                    User = settings ?? user.Settings,
                    Toolbox = toolbox
                };
            }

            return new Transfer.EditorSettings
            {
                IsSystemInitialised = false,
                User = settings ?? user.Settings
            };
        }

        private async Task<RobotType> RetrieveRobotTypeForUser(User? user)
        {
            RobotType? robotType = null;
            if (!string.IsNullOrEmpty(user?.Settings.RobotTypeId))
            {
                robotType = await this.session.LoadAsync<RobotType>(user.Settings.RobotTypeId);
            }

            if (robotType == null)
            {
                robotType = await this.session.Query<RobotType>()
                    .FirstOrDefaultAsync(rt => rt.IsDefault);
            }

            return robotType;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<ExecutionResult<Common.TokenSession>>> Post(Transfer.User? user)
        {
            if (user == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing session details"
                });
            }

            this._logger.LogInformation($"Starting new session for '{user.Name}'");
            Commands.CommandBase command;
            if (user.Role == "robot")
            {
                command = new Commands.StartRobotSession
                {
                    Password = user.Password,
                    Name = user.Name
                };
            }
            else if (!string.IsNullOrEmpty(user.Token))
            {
                command = new Commands.StartUserSessionViaToken
                {
                    Token = user.Token
                };
            }
            else
            {
                command = new Commands.StartUserSession
                {
                    Password = user.Password,
                    Name = user.Name
                };
            }

            return await this.ApplyCommand(command).ConfigureAwait(false);
        }

        [HttpPost("settings")]
        public async Task<ActionResult<ExecutionResult<Transfer.EditorSettings>>> PostSettings(Transfer.EditorSettings settings)
        {
            if (settings == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing settings"
                });
            }

            var user = await this.LoadUser(this.session).ConfigureAwait(false);
            if (user == null) return NotFound();

            this._logger.LogInformation($"Updating settings for '{user.Name}'");
            var command = new Commands.StoreSettings
            {
                UserId = user.Id,
                Settings = settings.User
            };

            return await this.commandManager.ExecuteForHttp(command, async s =>
            {
                var settings = await this.PrepareSettings(user, s);
                return settings;
            });
        }

        [HttpPut]
        public async Task<ActionResult<ExecutionResult>> Put()
        {
            var user = await this.LoadUser(this.session).ConfigureAwait(false);
            if (user == null) return NotFound();

            var command = new Commands.RenewSession { UserId = user.Id };
            return await this.commandManager.ExecuteForHttp(command);
        }

        private async Task<ActionResult<ExecutionResult<Common.TokenSession>>> ApplyCommand(Commands.CommandBase command)
        {
            var errors = await commandManager.ValidateAsync(command).ConfigureAwait(false);
            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    this._logger.LogInformation(error.Error);
                }

                return new BadRequestObjectResult(new ExecutionResult<Common.TokenSession>
                {
                    ValidationErrors = new[] { new CommandError(0, "Unable to validate session details") }
                });
            }

            var rawResult = (await commandManager.ApplyAsync(command).ConfigureAwait(false));
            if (!rawResult.WasSuccessful)
            {
                return new ObjectResult(new ExecutionResult<Common.TokenSession>
                {
                    ExecutionErrors = rawResult.ToErrors()
                })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

            var result = rawResult.As<Session>();
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(this.jwtSecret);
            var expiry = result.Output?.WhenExpires ?? DateTime.UtcNow;
            var role = UserRole.Student;
            if (result.Output != null)
            {
                role = result.Output.IsRobot
                    ? UserRole.Robot
                    : (result.Output.Role ?? UserRole.Student);
            }
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, result.Output?.UserId ?? string.Empty),
                    new Claim(ClaimTypes.Role, role.ToString()),
                    new Claim("SessionId", result.Output?.Id ?? string.Empty)
                }),
                Expires = expiry,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            await commandManager.CommitAsync().ConfigureAwait(false);
            return ExecutionResult.New(new Common.TokenSession
            {
                Token = tokenHandler.WriteToken(token),
                Expires = expiry,
                Role = (result.Output?.Role.ToString() ?? "Unknown")
            });
        }
        */
    }
}