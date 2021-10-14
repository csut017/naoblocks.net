using NaoBlocks.Common;
using NaoBlocks.Core.Commands.Helpers;
using NaoBlocks.Core.Models;
using Newtonsoft.Json;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class StoreDefaultAddress
        : CommandBase<SystemValues>
    {
        public string? Address { get; set; }

        public override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.Address))
            {
                errors.Add(this.GenerateError($"Address is required for storing a default site address"));
            }

            return Task.FromResult(errors.AsEnumerable());
        }

        protected async override Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var settings = await session.Query<SystemValues>()
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
            if (settings == null)
            {
                settings = new SystemValues();
                await session.StoreAsync(settings).ConfigureAwait(false);
            }

            settings.DefaultAddress = this.Address ?? string.Empty;
            return this.Result(settings);
        }
    }
}