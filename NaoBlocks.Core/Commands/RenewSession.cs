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
    public class RenewSession
        : CommandBase<Session>
    {
        [JsonIgnore]
        public Session? Session { get; set; }

        public string? UserId { get; set; }

        public async override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.UserId))
            {
                errors.Add(this.GenerateError($"User ID is required"));
            }

            if (!errors.Any())
            {
                this.Session = await session.Query<Session>()
                    .FirstOrDefaultAsync(u => u.UserId == this.UserId && u.WhenExpires > this.WhenExecuted)
                    .ConfigureAwait(false);
                if (this.Session == null)
                {
                    errors.Add(this.GenerateError("User does not have a current session"));
                }
            }

            return errors.AsEnumerable();
        }

        protected override Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (this.Session == null) throw new InvalidCallOrderException("ValidateAsync must be called first");

            this.Session.WhenExpires = this.WhenExecuted.AddDays(1);
            CommandResult result = this.Result(this.Session);
            return Task.FromResult(result);
        }
    }
}