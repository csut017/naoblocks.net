using NaoBlocks.Core.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class DeleteUserCommand
        : CommandBase
    {
        private User? person;

        public string? Name { get; set; }

        public UserRole Role { get; set; } = UserRole.Student;

        public async override Task<IEnumerable<string>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add($"{this.Role} name is required");
            }

            if (!errors.Any())
            {
                this.person = await session.Query<User>()
                                            .FirstOrDefaultAsync(u => u.Name == this.Name && u.Role == this.Role)
                                            .ConfigureAwait(false);
                if (person == null) errors.Add($"{this.Role} {this.Name} does not exist");
            }

            return errors.AsEnumerable();
        }

        protected override Task DoApplyAsync(IAsyncDocumentSession? session, CommandResult? result)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            session.Delete(this.person);
            return Task.CompletedTask;
        }
    }
}