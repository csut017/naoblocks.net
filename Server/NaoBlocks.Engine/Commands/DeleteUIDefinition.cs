using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command to delete a UI definition.
    /// </summary>
    public class DeleteUIDefinition
        : CommandBase
    {
        private UIDefinition? definition;

        /// <summary>
        /// Gets or sets the name of the definition.
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
            this.definition = await session.Query<UIDefinition>()
                .FirstOrDefaultAsync(d => d.Name == this.Name)
                .ConfigureAwait(false);
            if (this.definition == null)
            {
                errors.Add(GenerateError($"Definition '{this.Name}' does not exist"));
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

            if (string.IsNullOrEmpty(this.Name))
            {
                errors.Add(GenerateError("Definition name is required"));
            }
            else
            {
                this.definition = await session.Query<UIDefinition>()
                    .FirstOrDefaultAsync(d => d.Name == this.Name)
                    .ConfigureAwait(false);
                if (this.definition == null)
                {
                    errors.Add(GenerateError($"Definition '{this.Name}' does not exist"));
                }
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
            ValidateExecutionState(this.definition);
            session.Delete(this.definition);
            return Task.FromResult(CommandResult.New(this.Number, this.definition!));
        }
    }
}