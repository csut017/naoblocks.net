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
    public class StoreSnapshot
        : CommandBase<Snapshot>
    {
        public string? Source { get; set; }

        public string? State { get; set; }

        [JsonIgnore]
        public User? User { get; set; }

        public string? UserId { get; set; }

        public IList<SnapshotValue> Values { get; } = new List<SnapshotValue>();

        public async override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.State))
            {
                errors.Add(this.GenerateError($"State is required for storing a snapshot"));
            }

            if (string.IsNullOrWhiteSpace(this.UserId))
            {
                errors.Add(this.GenerateError($"UserID is required for storing a snapshot"));
            }

            if (!errors.Any())
            {
                this.User = await session.Query<User>().FirstOrDefaultAsync(u => u.Id == this.UserId).ConfigureAwait(false);
                if (this.User == null)
                {
                    errors.Add(this.GenerateError($"User {this.UserId} does not exist"));
                }
            }

            return errors.AsEnumerable();
        }

        protected async override Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (this.User == null) throw new InvalidCallOrderException();

            var snapshot = new Snapshot
            {
                Source = this.Source ?? "Unknown",
                State = this.State ?? string.Empty,
                User = this.User,
                UserId = this.UserId ?? string.Empty,
                WhenAdded = this.WhenExecuted
            };
            foreach (var value in this.Values)
            {
                snapshot.Values.Add(value);
            }

            await session.StoreAsync(snapshot).ConfigureAwait(false);

            return this.Result(snapshot);
        }
    }
}