using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command to delete a robot type.
    /// </summary>
    public class DeleteRobotType
        : RobotTypeCommandBase
    {
        private RobotType? robotType;

        /// <summary>
        /// Gets or sets the robot type name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Validates that the robot type exists.
        /// </summary>
        /// <param name="session">The database session.</param>
        /// <returns>Any valdiation errors.</returns>
        public async override Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            this.robotType = await this.ValidateAndRetrieveRobotType(session, this.Name, errors).ConfigureAwait(false);

            if (!errors.Any() && (this.robotType != null))
            {
                var hasRobots = await session.Query<Robot>()
                    .AnyAsync(r => r.RobotTypeId == this.robotType.Id)
                    .ConfigureAwait(false);

                if (hasRobots)
                {
                    errors.Add(this.GenerateError($"Robot type {this.Name} has robot instances"));
                }
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Deletes the robot type.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        protected override Task<CommandResult> DoExecuteAsync(IDatabaseSession session)
        {
            ValidateExecutionState(this.robotType);
            session.Delete(this.robotType);
            return Task.FromResult(CommandResult.New(this.Number));
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
