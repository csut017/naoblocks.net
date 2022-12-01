using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command to update the custom values for a robot.
    /// </summary>
    public class UpdateCustomValuesForRobot
        : UserCommandBase
    {
        private Robot? robot;

        /// <summary>
        /// Gets or sets the machine name of the robot.
        /// </summary>
        public string? MachineName { get; set; }

        /// <summary>
        /// Gets or sets the custom values.
        /// </summary>
        public IEnumerable<NamedValue> Values { get; set; } = Array.Empty<NamedValue>();

        /// <summary>
        /// Attempts to restore the command from the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during restoration.</returns>
        public override async Task<IEnumerable<CommandError>> RestoreAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            this.robot = await session.Query<Robot>()
                .FirstOrDefaultAsync(r => r.MachineName == this.MachineName)
                .ConfigureAwait(false);
            if (this.robot == null)
            {
                errors.Add(this.GenerateError($"Unknown robot '{this.MachineName}'"));
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Validates the robot.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        /// <param name="engine">The <see cref="IExecutionEngine"/> to use.</param>
        public override async Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var errors = new List<CommandError>();

            if (string.IsNullOrWhiteSpace(this.MachineName))
            {
                errors.Add(this.GenerateError($"Machine name is required"));
            }
            else
            {
                this.robot = await session.Query<Robot>()
                    .FirstOrDefaultAsync(r => r.MachineName == this.MachineName)
                    .ConfigureAwait(false);
                if (this.robot == null)
                {
                    errors.Add(this.GenerateError($"Unknown robot '{this.MachineName}'"));
                }
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Updates the custom values in the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the command has not been validated.</exception>
        /// <param name="engine">The <see cref="IExecutionEngine"/> to use.</param>
        protected override Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            ValidateExecutionState(this.robot);
            var robotType = this.robot!;
            robotType.CustomValues.Clear();
            foreach (var value in this.Values)
            {
                robotType.CustomValues.Add(value);
            }

            return Task.FromResult(CommandResult.New(this.Number, robotType));
        }
    }
}