using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// Command for adding a new synchronization source.
    /// </summary>
    public class AddSynchronizationSource
        : CommandBase
    {
        /// <summary>
        /// Gets or sets the source's address.
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Gets or sets the source's name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Validates the values for the new source.
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

            if (string.IsNullOrWhiteSpace(this.Address))
            {
                errors.Add(this.GenerateError($"Address is required"));
            }

            if (!errors.Any() && await session.Query<SynchronizationSource>().AnyAsync(s => s.Name == this.Name).ConfigureAwait(false))
            {
                errors.Add(this.GenerateError($"Synchronization source with name {this.Name} already exists"));
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Adds the user to the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <param name="engine"></param>
        protected override async Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var syncSource = new SynchronizationSource
            {
                Name = this.Name!.Trim(),
                Address = this.Address!.Trim(),
                WhenAdded = this.WhenExecuted
            };
            await session.StoreAsync(syncSource).ConfigureAwait(false);
            return CommandResult.New(this.Number, syncSource);
        }
    }
}