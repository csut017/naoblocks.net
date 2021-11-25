using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command to set the default robot type.
    /// </summary>
    public class SetDefaultRobotType
        : RobotTypeCommandBase
    {
        private RobotType? robotType;

        /// <summary>
        /// Gets or sets the name of the default robot type.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Attempts to retrieve the robot type.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        public async override Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            this.robotType = await this.ValidateAndRetrieveRobotType(session, this.Name, errors).ConfigureAwait(false);
            return errors.AsEnumerable();
        }

        /// <summary>
        /// Updates the robot type in the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the command has not been validated.</exception>
        protected override async Task<CommandResult> DoExecuteAsync(IDatabaseSession session)
        {
            ValidateExecutionState(this.robotType);
            var existingDefaultRobotType = await session.Query<RobotType>()
                .FirstOrDefaultAsync(type => type.IsDefault);
            if (!this.robotType!.Id.Equals(existingDefaultRobotType?.Id))
            {
                if (existingDefaultRobotType != null)
                {
                    existingDefaultRobotType.IsDefault = false;
                }

                this.robotType!.IsDefault = true;
            }

            return CommandResult.New(this.Number, this.robotType!);
        }

        /// <summary>
        /// Attempts to restore the command from the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during restoration.</returns>
        public async override Task<IEnumerable<CommandError>> RestoreAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            this.robotType = await this.ValidateAndRetrieveRobotType(session, this.Name, errors).ConfigureAwait(false);
            return errors.AsEnumerable();
        }
    }
}
