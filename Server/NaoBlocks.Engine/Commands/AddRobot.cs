
using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command for adding a new robot.
    /// </summary>
    public class AddRobot
        : CommandBase
    {
        private RobotType? robotType;

        /// <summary>
        /// Gets or sets the friendly name of the robot.
        /// </summary>
        public string? FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the hashed password
        /// </summary>
        public Password HashedPassword { get; set; } = Data.Password.Empty;

        /// <summary>
        /// Gets or sets the machine name of the robot.
        /// </summary>
        public string? MachineName { get; set; }

        /// <summary>
        /// Gets or sets the plain-text password.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the robot type name.
        /// </summary>
        public string? Type { get; set; }

        public async override Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.MachineName))
            {
                errors.Add(this.GenerateError($"Machine name is required for a robot"));
            }

            if (string.IsNullOrWhiteSpace(this.FriendlyName))
            {
                this.FriendlyName = this.MachineName;
            }

            if (string.IsNullOrWhiteSpace(this.Type))
            {
                errors.Add(this.GenerateError($"Type is required for a robot"));
            }

            if (!errors.Any())
            {
                if (await session.Query<Robot>().AnyAsync(s => s.MachineName == this.MachineName).ConfigureAwait(false))
                {
                    errors.Add(this.GenerateError($"Robot with name {this.MachineName} already exists"));
                }

                this.robotType = await session.Query<RobotType>()
                    .FirstOrDefaultAsync(rt => rt.Name == this.Type)
                    .ConfigureAwait(false);
                if (this.robotType == null)
                {
                    errors.Add(this.GenerateError($"Unknown robot type {this.Type}"));
                }
            }

            this.HashedPassword = Data.Password.New(this.Password ?? string.Empty);
            this.Password = null;

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Adds the robot to the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        protected override async Task<CommandResult> DoExecuteAsync(IDatabaseSession session)
        {
            var robot = new Robot
            {
                MachineName = this.MachineName ?? "<Unknown>",
                FriendlyName = this.FriendlyName ?? "<Unknown>",
                Password = this.HashedPassword,
                IsInitialised = true,
                RobotTypeId = this.robotType!.Id ?? string.Empty,
                WhenAdded = this.WhenExecuted
            };
            await session.StoreAsync(robot).ConfigureAwait(false);
            return CommandResult.New(this.Number, robot);
        }

        /// <summary>
        /// Attempts to restore the command from the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during restoration.</returns>
        public async override Task<IEnumerable<CommandError>> RestoreAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            this.robotType = await session.Query<RobotType>()
                .FirstOrDefaultAsync(rt => rt.Name == this.Type)
                .ConfigureAwait(false);
            if (this.robotType == null)
            {
                errors.Add(this.GenerateError($"Unknown robot type {this.Type}"));
            }
            return errors.AsEnumerable();
        }
    }
}
