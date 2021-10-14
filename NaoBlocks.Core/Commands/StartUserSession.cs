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
    public class StartUserSession
        : CommandBase<Session>
    {
        [JsonIgnore]
        public string? Name { get; set; }

        [JsonIgnore]
        public string? Password { get; set; }

        public UserRole Role { get; set; } = UserRole.Student;

        public string? UserId { get; set; }

        public async override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add(this.GenerateError($"User name is required"));
            }

            if (this.Password == null)
            {
                errors.Add(this.GenerateError("Password is required"));
            }

            if (!errors.Any())
            {
                var user = await session.Query<User>()
                    .FirstOrDefaultAsync(u => u.Name == this.Name)
                    .ConfigureAwait(false);
                if (user == null)
                {
                    errors.Add(this.GenerateError("Unknown or invalid user"));
                }
                else if ((user.Password != null) && !user.Password.Verify(this.Password))
                {
                    errors.Add(this.GenerateError("Unknown or invalid user"));
                }
                else
                {
                    this.UserId = user.Id;
                    this.Role = user.Role;
                }
            }

            return errors.AsEnumerable();
        }

        protected override async Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (string.IsNullOrEmpty(this.UserId)) throw new InvalidCallOrderException("ValidateAsync must be called first");

            var now = this.WhenExecuted;
            var userId = this.UserId ?? "<Unknown>";
            var existing = await session.Query<Session>()
                .FirstOrDefaultAsync(us => us.UserId == userId && us.WhenExpires > now)
                .ConfigureAwait(false);
            if (existing == null)
            {
                var newSession = new Session
                {
                    Role = this.Role,
                    UserId = userId,
                    WhenAdded = now,
                    WhenExpires = now.AddDays(1)
                };
                await session.StoreAsync(newSession).ConfigureAwait(false);
                return this.Result(newSession);
            }

            existing.WhenExpires = now.AddDays(1);
            return this.Result(existing);
        }
    }
}