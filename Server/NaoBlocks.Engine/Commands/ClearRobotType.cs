using NaoBlocks.Common;
using NaoBlocks.Engine.Data;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// Clears data related to a robot type.
    /// </summary>
    public class ClearRobotType
        : RobotTypeCommandBase
    {
        private RobotType? robotType;

        /// <summary>
        /// Gets or sets a flag indicating that custom values should be wiped.
        /// </summary>
        public bool IncludeCustomValues { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating that logging templates should be wiped.
        /// </summary>
        public bool IncludeLoggingTemplates { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating that toolboxes should be wiped.
        /// </summary>
        public bool IncludeToolboxes { get; set; }

        /// <summary>
        /// Gets or sets the name of the robot type.
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
            this.robotType = await this.ValidateAndRetrieveRobotType(session, this.Name, errors).ConfigureAwait(false);
            return errors.AsEnumerable();
        }

        /// <summary>
        /// Attempts to retrieve the existing robot type and validates the changes.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        /// <param name="engine"></param>
        public override async Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add(this.GenerateError($"Name is required"));
            }

            if (!errors.Any())
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
        protected override Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            ValidateExecutionState(this.robotType);
            if (this.IncludeCustomValues) this.robotType!.CustomValues.Clear();
            if (this.IncludeLoggingTemplates) this.robotType!.LoggingTemplates.Clear();
            if (this.IncludeToolboxes) this.robotType!.Toolboxes.Clear();
            return Task.FromResult(CommandResult.New(this.Number, this.robotType!));
        }
    }
}