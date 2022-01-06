using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Data = NaoBlocks.Engine.Data;
using Generators = NaoBlocks.Engine.Generators;
using Transfer = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Controllers
{
    /// <summary>
    /// A controller for working with sessions.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
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
        /// <response code="401">If the request is not authenticated.</response>
        /// <response code="404">If the current user cannot be found.</response>
        [HttpDelete]
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
        /// <response code="401">If the request is not authenticated.</response>
        /// <response code="404">If the current user cannot be found.</response>
        [HttpGet]
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

        /// <summary>
        /// Retrieves the editor settings for the current user.
        /// </summary>
        /// <returns>The editor settings for the current user.</returns>
        [HttpGet("settings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Transfer.EditorSettings>> GetSettings()
        {
            var user = await this.LoadUserAsync(this.executionEngine)
                .ConfigureAwait(false);
            if (user == null) return NotFound();

            var settings = await this.PrepareSettings(user);
            return settings;
        }

        /// <summary>
        /// Prepares the settings for a user.
        /// </summary>
        /// <param name="user">The user to retrieve the settings for.</param>
        /// <param name="settings">The current settings.</param>
        /// <returns>The <see cref="Transfer.EditorSettings"/> instance.</returns>
        private async Task<Transfer.EditorSettings> PrepareSettings(
            Data.User user,
            Data.UserSettings? settings = null)
        {
            var robotType = await this.RetrieveRobotTypeForUser(user);
            if (robotType != null)
            {
                var toolbox = await this.executionEngine
                    .Generator<Generators.UserToolbox>()
                    .GenerateAsync(ReportFormat.Text, user, robotType)
                    .ConfigureAwait(false);
                using var reader = new StreamReader(toolbox.Item1);
                return new Transfer.EditorSettings
                {
                    CanConfigure = string.IsNullOrEmpty((settings ?? user.Settings).CustomBlockSet),
                    IsSystemInitialised = true,
                    User = settings ?? user.Settings,
                    Toolbox = await reader.ReadToEndAsync()
                };
            }

            return new Transfer.EditorSettings
            {
                IsSystemInitialised = false,
                User = settings ?? user.Settings
            };
        }

        /// <summary>
        /// Attempt to retrieve the robot type for a user.
        /// </summary>
        /// <param name="user">The user to retrieve the robot type for.</param>
        /// <returns>The <see cref="Data.RobotType"/> if found, null otherwise.</returns>
        private async Task<Data.RobotType?> RetrieveRobotTypeForUser(
            Data.User? user)
        {
            Data.RobotType? robotType = null;
            if (!string.IsNullOrEmpty(user?.Settings.RobotTypeId))
            {
                robotType = await this.executionEngine
                    .Query<RobotTypeData>()
                    .RetrieveByIdAsync(user.Settings.RobotTypeId)
                    .ConfigureAwait(false);
            }

            if (robotType == null)
            {
                robotType = await this.executionEngine
                    .Query<RobotTypeData>()
                    .RetrieveDefaultAsync()
                    .ConfigureAwait(false);
            }

            return robotType;
        }

        /// <summary>
        /// Starts a new user session.
        /// </summary>
        /// <param name="user">The details of the user.</param>
        /// <returns>The session details.</returns>
        /// <response code="200">Returns the session details.</response>
        /// <response code="401">If there is an error in the request (e.g. invalid password or unknown login.)</response>
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ExecutionResult<UserSessionResult>>> Post(Transfer.User? user)
        {
            if (user == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing session details"
                });
            }

            this._logger.LogInformation($"Starting new session for '{user.Name}'");
            CommandBase command;
            if (user.Role == "robot")
            {
                command = new StartRobotSession
                {
                    Password = user.Password,
                    Name = user.Name
                };
            }
            else if (!string.IsNullOrEmpty(user.Token))
            {
                command = new StartUserSessionViaToken
                {
                    Token = user.Token
                };
            }
            else
            {
                command = new StartUserSession
                {
                    Password = user.Password,
                    Name = user.Name
                };
            }

            return await this.ApplyCommand(command)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Updates the current user's settings.
        /// </summary>
        /// <param name="settings">The updated settings.</param>
        /// <returns>The result of execution.</returns>
        [HttpPost("settings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ExecutionResult<Transfer.EditorSettings>>> PostSettings(Transfer.EditorSettings? settings)
        {
            if (settings == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing settings"
                });
            }

            var user = await this.LoadUserAsync(this.executionEngine).ConfigureAwait(false);
            if (user == null) return NotFound();

            this._logger.LogInformation($"Updating settings for '{user.Name}'");
            var command = new StoreSettings
            {
                UserName = user.Name,
                Settings = settings.User
            };

            return await this.executionEngine.ExecuteForHttp<Data.UserSettings, Transfer.EditorSettings>(command, s =>
            {
                var settings = this.PrepareSettings(user, s).Result;
                return settings;
            });
        }

        /// <summary>
        /// Renews a user session.
        /// </summary>
        /// <returns>The result of execution.</returns>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ExecutionResult>> Put()
        {
            var user = await this.LoadUserAsync(this.executionEngine).ConfigureAwait(false);
            if (user == null) return NotFound();

            var command = new RenewSession { UserName = user.Name };
            return await this.executionEngine.ExecuteForHttp(command);
        }

        private async Task<ActionResult<ExecutionResult<UserSessionResult>>> ApplyCommand(CommandBase command)
        {
            var errors = await executionEngine.ValidateAsync(command)
                .ConfigureAwait(false);
            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    this._logger.LogInformation(error.Error);
                }

                return new BadRequestObjectResult(new ExecutionResult<UserSessionResult>
                {
                    ValidationErrors = new[] { new CommandError(0, "Unable to validate session details") }
                });
            }

            var rawResult = (await executionEngine.ExecuteAsync(command)
                .ConfigureAwait(false));
            if (!rawResult.WasSuccessful)
            {
                return new ObjectResult(new ExecutionResult<UserSessionResult>
                {
                    ExecutionErrors = rawResult.ToErrors()
                })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

            var result = rawResult.As<Data.Session>();
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(this.jwtSecret);
            var expiry = result.Output?.WhenExpires ?? DateTime.UtcNow;
            Data.UserRole role = result!.Output!.IsRobot
                ? Data.UserRole.Robot
                : result.Output.Role ?? Data.UserRole.Student;
            var name = result.Output.UserId ?? "<unknown>";
            var sessionId = result.Output?.Id ?? string.Empty;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, name),
                    new Claim(ClaimTypes.Role, role.ToString()),
                    new Claim("SessionId", sessionId)
                }),
                Expires = expiry,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            await executionEngine.CommitAsync().ConfigureAwait(false);
            var remaining = expiry.Subtract(this.CurrentTimeFunc());
            return ExecutionResult.New(new UserSessionResult
            {
                Token = tokenHandler.WriteToken(token),
                TimeRemaining = Convert.ToInt32(remaining.TotalMinutes),
                Role = role.ToString()
            });
        }
    }
}