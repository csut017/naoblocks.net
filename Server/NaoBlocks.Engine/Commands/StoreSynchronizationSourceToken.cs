using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command to store a token for a synchronization source.
    /// </summary>
    [Transient]
    public class StoreSynchronizationSourceToken
        : CommandBase
    {
        private SynchronizationSource? source;

        /// <summary>
        /// Gets or sets the source's name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the new token to use in future.
        /// </summary>
        public string? Token { get; set; }

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
        /// Attempts to retrieve the existing synchronization source and validates the changes.
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

            if (string.IsNullOrWhiteSpace(this.Token))
            {
                errors.Add(this.GenerateError($"Token is required"));
            }

            if (!errors.Any())
            {
                this.source = await session.Query<SynchronizationSource>()
                    .FirstOrDefaultAsync(s => s.Name == this.Name)
                    .ConfigureAwait(false);
                if (this.source == null)
                {
                    errors.Add(GenerateError($"Synchronization source '{this.Name}' does not exist"));
                }
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Updates the synchronization source in the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the command has not been validated.</exception>
        /// <param name="engine"></param>
        protected override Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            ValidateExecutionState(this.source);
            this.source!.Tokens ??= new List<MachineToken>();
            var existing = this.source!.Tokens.FirstOrDefault(m => m.Machine == Environment.MachineName);
            if (existing == null)
            {
                existing = new()
                {
                    Machine = Environment.MachineName,
                };
                this.source.Tokens.Add(existing);
            }
            existing.Token = ProtectedDataUtility.EncryptValue(this.Token!);

            return Task.FromResult(CommandResult.New(this.Number, this.source!));
        }
    }
}