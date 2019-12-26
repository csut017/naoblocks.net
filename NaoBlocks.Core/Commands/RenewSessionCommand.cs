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
    public class RenewSessionCommand
        : CommandBase<Session>
    {
        [JsonIgnore]
        public Session? Session { get; set; }

        public string? UserId { get; set; }

        public async override Task<IEnumerable<string>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(this.UserId))
            {
                errors.Add($"User ID is required");
            }

            if (!errors.Any())
            {
                this.Session = await session.Query<Session>()
                    .FirstOrDefaultAsync(u => u.UserId == this.UserId && u.WhenExpires > this.WhenExecuted)
                    .ConfigureAwait(false);
                if (this.Session == null)
                {
                    errors.Add("User does not have a current session");
                }
            }

            return errors.AsEnumerable();
        }

        protected override Task DoApplyAsync(IAsyncDocumentSession? session, CommandResult? result)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (this.Session == null) throw new InvalidCallOrderException("ValidateAsync must be called first");

            this.Session.WhenExpires = this.WhenExecuted.AddDays(1);
            this.Output = this.Session;
            return Task.CompletedTask;
        }
    }
}