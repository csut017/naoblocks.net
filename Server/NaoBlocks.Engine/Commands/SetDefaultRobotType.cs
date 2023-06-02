using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command to set the default robot type.
    /// </summary>
    [CommandTarget(CommandTarget.RobotType)]
    public class SetDefaultRobotType
        : RobotTypeCommandBase
    {
        private RobotType? robotType;

        /// <summary>
        /// Gets or sets whether to ignore any missing robot and only check if during execution.
        /// </summary>
        public bool IgnoreMissingRobotType { get; set; }

        /// <summary>
        /// Gets or sets the name of the default robot type.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Attempts to restore the command from the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during restoration.</returns>
        public override async Task<IEnumerable<CommandError>> RestoreAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            if (!this.IgnoreMissingRobotType)
            {
                this.robotType = await this.ValidateAndRetrieveRobotType(session, this.Name, errors).ConfigureAwait(false);
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Attempts to retrieve the robot type.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        /// <param name="engine"></param>
        public override async Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var errors = new List<CommandError>();
            if (!this.IgnoreMissingRobotType)
            {
                this.robotType = await this.ValidateAndRetrieveRobotType(session, this.Name, errors).ConfigureAwait(false);
            }
            return errors.AsEnumerable();
        }

        /// <summary>
        /// Updates the robot type in the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the command has not been validated.</exception>
        /// <param name="engine"></param>
        protected override async Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            if (this.IgnoreMissingRobotType)
            {
                this.robotType = await this.ValidateAndRetrieveRobotType(session, this.Name, new List<CommandError>()).ConfigureAwait(false);
                if (this.robotType == null) this.robotType = session.GetFromCache<RobotType>(this.Name ?? String.Empty);
            }

            ValidateExecutionState(this.robotType);
            var existingDefaultRobotType = await session.Query<RobotType>()
                .FirstOrDefaultAsync(type => type.IsDefault);
            if (!this.robotType!.Id.Equals(existingDefaultRobotType?.Id))
            {
                if (existingDefaultRobotType != null)
                {
                    existingDefaultRobotType.IsDefault = false;
                    existingDefaultRobotType.WhenLastUpdated = this.WhenExecuted;
                }

                this.robotType!.IsDefault = true;
            }

            this.robotType.WhenLastUpdated = this.WhenExecuted;
            return CommandResult.New(this.Number, this.robotType!);
        }
    }
}