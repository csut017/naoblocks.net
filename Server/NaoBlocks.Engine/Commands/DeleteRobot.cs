using NaoBlocks.Common;
using NaoBlocks.Engine.Data;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command to delete a robot.
    /// </summary>
    [CommandTarget(CommandTarget.Robot)]
    public class DeleteRobot
        : RobotCommandBase
    {
        private Robot? robot;

        /// <summary>
        /// Gets or sets the robot name.
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
            this.robot = await this.ValidateAndRetrieveRobot(session, this.Name, errors).ConfigureAwait(false);
            return errors.AsEnumerable();
        }

        /// <summary>
        /// Validates that the robot exists.
        /// </summary>
        /// <param name="session">The database session.</param>
        /// <returns>Any valdiation errors.</returns>
        /// <param name="engine"></param>
        public override async Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var errors = new List<CommandError>();
            this.robot = await this.ValidateAndRetrieveRobot(session, this.Name, errors).ConfigureAwait(false);
            return errors.AsEnumerable();
        }

        /// <summary>
        /// Deletes the robot.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <param name="engine"></param>
        protected override Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            ValidateExecutionState(this.robot);
            session.Delete(this.robot);
            return Task.FromResult(CommandResult.New(this.Number));
        }
    }
}