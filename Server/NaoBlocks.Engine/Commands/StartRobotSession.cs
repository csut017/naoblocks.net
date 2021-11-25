using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Newtonsoft.Json;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command for starting a robot session.
    /// </summary>
    public class StartRobotSession
        : CommandBase
    {
        /// <summary>
        /// Gets or sets the robot name.
        /// </summary>
        [JsonIgnore]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the password to check.
        /// </summary>
        [JsonIgnore]
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the robot id.
        /// </summary>
        /// <remarks>
        /// This property is needed for reloading as the name and password are not stored.
        /// </remarks>
        public string? RobotId { get; set; }

        /// <summary>
        /// Validates the robot via their name and password.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        public async override Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add(this.GenerateError($"Robot name is required"));
            }

            if (string.IsNullOrWhiteSpace(this.Password))
            {
                errors.Add(this.GenerateError($"Password is required"));
            }

            if (!errors.Any())
            {
                var robot = await session.Query<Robot>()
                    .FirstOrDefaultAsync(r => r.MachineName == this.Name)
                    .ConfigureAwait(false);
                if (robot == null)
                {
                    errors.Add(this.GenerateError("Unknown or invalid robot"));
                }
                else if ((robot.Password != null) && !robot.Password.Verify(this.Password))
                {
                    errors.Add(this.GenerateError("Unknown or invalid robot"));
                }
                else
                {
                    this.RobotId = robot.Id;
                }
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Stores the program in the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the command has not been validated.</exception>
        protected async override Task<CommandResult> DoExecuteAsync(IDatabaseSession session)
        {
            ValidateExecutionState(this.RobotId);
            var now = this.WhenExecuted;
            var robotId = this.RobotId ?? "<Unknown>";
            var existing = await session.Query<Session>()
                .FirstOrDefaultAsync(us => us.UserId == robotId && us.WhenExpires > now)
                .ConfigureAwait(false);
            if (existing == null)
            {
                var newSession = new Session
                {
                    Role = UserRole.Robot,
                    IsRobot = true,
                    UserId = robotId,
                    WhenAdded = now,
                    WhenExpires = now.AddDays(1)
                };
                await session.StoreAsync(newSession).ConfigureAwait(false);
                return CommandResult.New(this.Number, newSession);
            }

            existing.WhenExpires = now.AddDays(1);
            return CommandResult.New(this.Number, existing);
        }
    }
}
