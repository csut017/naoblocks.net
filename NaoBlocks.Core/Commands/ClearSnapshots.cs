using NaoBlocks.Core.Commands.Helpers;
using NaoBlocks.Core.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class ClearSnapshots
        : CommandBase
    {
        private User? person;

        public string? Name { get; set; }

        public UserRole? Role { get; set; }

        public async override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<CommandError>();
            this.person = await this.ValidateAndRetrieveUser(session, this.Name, this.Role, errors)
                .ConfigureAwait(false);
            return errors.AsEnumerable();
        }

        protected override async Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (this.person == null) throw new InvalidOperationException("ValidateAsync must be called first");

            var snapshots = await session.Query<Snapshot>()
                .Where(cp => cp.UserId == this.person.Id)
                .ToListAsync()
                .ConfigureAwait(false);
            foreach (var snapshot in snapshots)
            {
                session.Delete(snapshot);
            }

            return CommandResult.New(this.Number);
        }
    }
}
