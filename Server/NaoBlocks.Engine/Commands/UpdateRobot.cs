using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command to update a robot.
    /// </summary>
    public class UpdateRobot
        : RobotCommandBase
    {
        private Robot? robot;

        private RobotType? robotType;

        /// <summary>
        /// Gets or sets the friendly name.
        /// </summary>
        public string? FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the hashed password.
        /// </summary>
        public Password? HashedPassword { get; set; }

        /// <summary>
        /// Gets or sets the machine name.
        /// </summary>
        public string? MachineName { get; set; }

        /// <summary>
        /// Gets or sets the plain type password
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the type of the robot.
        /// </summary>
        public string? RobotType { get; set; }

        /// <summary>
        /// Validates that the robot exists.
        /// </summary>
        /// <param name="session">The database session.</param>
        /// <returns>Any valdiation errors.</returns>
        /// <param name="engine"></param>
        public async override Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.MachineName))
            {
                errors.Add(this.GenerateError($"Machine name is required"));
            }

            if (!errors.Any())
            {
                this.robot = await this.ValidateAndRetrieveRobot(session, this.MachineName, errors).ConfigureAwait(false);
            }

            if (this.Password != null)
            {
                this.HashedPassword = Data.Password.New(this.Password);
                this.Password = null;
            }

            if (!string.IsNullOrEmpty(this.RobotType))
            {
                this.robotType = await session.Query<RobotType>()
                    .FirstOrDefaultAsync(rt => rt.Name == this.RobotType)
                    .ConfigureAwait(false);
                if (this.robotType == null)
                {
                    errors.Add(this.GenerateError($"Unknown robot type {this.RobotType}"));
                }
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Updates the robot in the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the command has not been validated.</exception>
        /// <param name="engine"></param>
        protected override Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            ValidateExecutionState(this.robot);
            if (!string.IsNullOrEmpty(this.FriendlyName) && (this.FriendlyName != this.robot!.FriendlyName)) this.robot.FriendlyName = this.FriendlyName;
            if (this.HashedPassword != null)
            {
                this.robot!.Password = this.HashedPassword;
                this.robot.IsInitialised = true;
            }

            if (!string.IsNullOrEmpty(this.RobotType))
            {
                this.robot!.RobotTypeId = this.robotType!.Id;
            }

            this.robot!.WhenLastUpdated = DateTime.UtcNow;
            return Task.FromResult(CommandResult.New(this.Number, this.robot!));
        }

        /// <summary>
        /// Attempts to restore the command from the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during restoration.</returns>
        public async override Task<IEnumerable<CommandError>> RestoreAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            this.robot = await this.ValidateAndRetrieveRobot(session, this.MachineName, errors).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(this.RobotType))
            {
                this.robotType = await session.Query<RobotType>()
                    .FirstOrDefaultAsync(rt => rt.Name == this.RobotType)
                    .ConfigureAwait(false);
                if (this.robotType == null)
                {
                    errors.Add(this.GenerateError($"Robot type {this.RobotType} is missing"));
                }
            }
            return errors.AsEnumerable();
        }
    }
}
