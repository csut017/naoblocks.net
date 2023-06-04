using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command to delete a synchronization source.
    /// </summary>
    public class DeleteSynchronizationSource
        : CommandBase
    {
        private SynchronizationSource? source;

        /// <summary>
        /// Gets or sets the source's name.
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
            this.source = await session.Query<SynchronizationSource>()
                .FirstOrDefaultAsync(s => s.Name == this.Name)
                .ConfigureAwait(false);
            return errors.AsEnumerable();
        }

        /// <summary>
        /// Attempts to retrieve the existing source.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        /// <param name="engine"></param>
        public override async Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var errors = new List<CommandError>();
            this.source = await session.Query<SynchronizationSource>()
                .FirstOrDefaultAsync(s => s.Name == this.Name)
                .ConfigureAwait(false);
            if (this.source == null)
            {
                errors.Add(GenerateError($"Synchronization source '{this.Name}' does not exist"));
            }
            return errors.AsEnumerable();
        }

        /// <summary>
        /// Deletes the source.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <param name="engine"></param>
        protected override Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            ValidateExecutionState(this.source);
            session.Delete(this.source);
            return Task.FromResult(CommandResult.New(this.Number));
        }
    }
}