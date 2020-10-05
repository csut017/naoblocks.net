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
    public class FinishSession
        : CommandBase
    {
        public string? UserId { get; set; }

        public override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.UserId))
            {
                errors.Add(this.GenerateError($"User ID is required"));
            }

            return Task.FromResult(errors.AsEnumerable());
        }

        protected override async Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var now = this.WhenExecuted;
            var userId = this.UserId ?? "<Unknown>";
            var existing = await session.Query<Session>()
                .FirstOrDefaultAsync(us => us.UserId == userId && us.WhenExpires > now)
                .ConfigureAwait(false);
            if (existing != null)
            {
                existing.WhenExpires = now.AddMinutes(-1);
                return CommandResult.New(this.Number, existing);
            }

            return CommandResult.New(this.Number);
        }
    }
}