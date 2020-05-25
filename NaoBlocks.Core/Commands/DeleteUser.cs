using NaoBlocks.Core.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class DeleteUser
        : CommandBase
    {
        private User? person;

        public string? Name { get; set; }

        public UserRole? Role { get; set; }

        public async override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<CommandError>();
            var roleName = this.Role?.ToString() ?? "User";
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add(this.Error($"{roleName} name is required"));
            }

            if (!errors.Any())
            {
                this.person = this.Role == null
                    ? await session.Query<User>()
                                .FirstOrDefaultAsync(u => u.Name == this.Name)
                                .ConfigureAwait(false)
                    : await session.Query<User>()
                                .FirstOrDefaultAsync(u => u.Name == this.Name && u.Role == this.Role)
                                .ConfigureAwait(false);
                if (person == null) errors.Add(this.Error($"{roleName} {this.Name} does not exist"));
            }

            return errors.AsEnumerable();
        }

        protected override Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            session.Delete(this.person);
            return Task.FromResult(CommandResult.New(this.Number));
        }
    }
}