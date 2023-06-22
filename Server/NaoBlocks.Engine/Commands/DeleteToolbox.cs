using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command to delete a toolbox from a robot type.
    /// </summary>
    public class DeleteToolbox
        : RobotTypeCommandBase
    {
        private RobotType? robotType;

        /// <summary>
        /// Gets or sets the robot type name.
        /// </summary>
        public string? RobotTypeName { get; set; }

        /// <summary>
        /// Gets or sets the toolbox name.
        /// </summary>
        public string? ToolboxName { get; set; }

        /// <summary>
        /// Attempts to restore the command from the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during restoration.</returns>
        public override async Task<IEnumerable<CommandError>> RestoreAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            this.robotType = await this.ValidateAndRetrieveRobotType(session, this.RobotTypeName, errors).ConfigureAwait(false);
            return errors.AsEnumerable();
        }

        /// <summary>
        /// Validates that the robot type exists.
        /// </summary>
        /// <param name="session">The database session.</param>
        /// <returns>Any valdiation errors.</returns>
        /// <param name="engine"></param>
        public override async Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var errors = new List<CommandError>();
            this.robotType = await this.ValidateAndRetrieveRobotType(session, this.RobotTypeName, errors).ConfigureAwait(false);

            if (string.IsNullOrEmpty(this.ToolboxName))
            {
                errors.Add(GenerateError("Toolbox name is required"));
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Deletes the robot type.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <param name="engine"></param>
        protected override Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            ValidateExecutionState(this.robotType);
            var toolbox = this.robotType!.Toolboxes.FirstOrDefault(t => t.Name == this.ToolboxName);
            if (toolbox != null)
            {
                this.robotType!.Toolboxes.Remove(toolbox);
                if (toolbox.IsDefault)
                {
                    var nextDefault = this.robotType!.Toolboxes.FirstOrDefault();
                    if (nextDefault != null) nextDefault.IsDefault = true;
                }
            }

            return Task.FromResult(CommandResult.New(this.Number, this.robotType));
        }
    }
}