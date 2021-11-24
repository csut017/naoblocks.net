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
        /// Validates that the robot exists.
        /// </summary>
        /// <param name="session">The database session.</param>
        /// <returns>Any valdiation errors.</returns>
        public async override Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session)
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

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Updates the robot in the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the command has not been validated.</exception>
        protected override Task<CommandResult> DoExecuteAsync(IDatabaseSession session)
        {
            this.ValidateExecutionState(this.robot);
            if (!string.IsNullOrEmpty(this.FriendlyName) && (this.FriendlyName != this.robot!.FriendlyName)) this.robot.FriendlyName = this.FriendlyName;
            if (this.HashedPassword != null)
            {
                this.robot!.Password = this.HashedPassword;
                this.robot.IsInitialised = true;
            }

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
            return errors.AsEnumerable();
        }
    }
}
