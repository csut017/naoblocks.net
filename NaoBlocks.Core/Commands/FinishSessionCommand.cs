using NaoBlocks.Core.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class FinishSessionCommand
        : CommandBase<Session>
    {
        public string? UserId { get; set; }

        public override Task<IEnumerable<string>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(this.UserId))
            {
                errors.Add($"User ID is required");
            }

            return Task.FromResult(errors.AsEnumerable());
        }

        protected override async Task DoApplyAsync(IAsyncDocumentSession? session, CommandResult? result)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var now = this.WhenExecuted;
            var userId = this.UserId ?? "<Unknown>";
            var existing = await session.Query<Session>()
                .FirstOrDefaultAsync(us => us.UserId == userId && us.WhenExpires > now)
                .ConfigureAwait(false);
            this.Output = null;
            if (existing != null)
            {
                existing.WhenExpires = now.AddMinutes(-1);
                this.Output = existing;
            }
        }
    }
}