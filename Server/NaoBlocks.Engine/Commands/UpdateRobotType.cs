using NaoBlocks.Common;
using NaoBlocks.Engine.Data;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command to update a robot type.
    /// </summary>
    [CommandTarget(CommandTarget.RobotType)]
    public class UpdateRobotType
        : RobotTypeCommandBase
    {
        private RobotType? robotType;

        /// <summary>
        /// Gets or sets whether this robot type allows direct logging.
        /// </summary>
        public bool? AllowDirectLogging { get; set; }

        /// <summary>
        /// Gets or sets the current name of the robot type.
        /// </summary>
        public string? CurrentName { get; set; }

        /// <summary>
        /// Gets or sets the new name of the robot type.
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
            this.robotType = await this.ValidateAndRetrieveRobotType(session, this.CurrentName, errors).ConfigureAwait(false);
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
            if (string.IsNullOrWhiteSpace(this.CurrentName))
            {
                errors.Add(this.GenerateError($"Current name is required"));
            }

            if (!errors.Any())
            {
                this.robotType = await this.ValidateAndRetrieveRobotType(session, this.CurrentName, errors).ConfigureAwait(false);
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
            if (!string.IsNullOrWhiteSpace(this.Name) && (this.Name != this.robotType!.Name)) this.robotType.Name = this.Name.Trim();
            if (this.AllowDirectLogging.HasValue) this.robotType!.AllowDirectLogging = this.AllowDirectLogging.Value;
            this.robotType!.WhenLastUpdated = this.WhenExecuted;

            return Task.FromResult(CommandResult.New(this.Number, this.robotType!));
        }
    }
}