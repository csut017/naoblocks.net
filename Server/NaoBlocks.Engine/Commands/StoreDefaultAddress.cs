﻿using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command for storing the default system address.
    /// </summary>
    public class StoreDefaultAddress
        : CommandBase
    {
        /// <summary>
        /// The default system address.
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Validates the default address.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        public override Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.Address))
            {
                errors.Add(this.GenerateError($"Address is required for storing a default site address"));
            }

            return Task.FromResult(errors.AsEnumerable());
        }

        /// <summary>
        /// Stores the snapshot in the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the command has not been validated.</exception>
        protected async override Task<CommandResult> DoExecuteAsync(IDatabaseSession session)
        {
            var settings = await session.Query<SystemValues>()
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
            if (settings == null)
            {
                settings = new SystemValues();
                await session.StoreAsync(settings).ConfigureAwait(false);
            }

            settings.DefaultAddress = this.Address ?? string.Empty;
            return CommandResult.New(this.Number, settings);
        }
    }
}
