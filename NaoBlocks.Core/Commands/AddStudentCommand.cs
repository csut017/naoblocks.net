using NaoBlocks.Core.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class AddStudentCommand
        : CommandBase
    {
        public string Name { get; set; }

        public async override Task<IEnumerable<string>> ValidateAsync(IAsyncDocumentSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add("Student name is required");
            }

            if (!errors.Any() && await session.Query<User>().AnyAsync(s => s.Name == this.Name).ConfigureAwait(false))
            {
                errors.Add($"Person with name {this.Name} already exists");
            }

            return errors.AsEnumerable();
        }

        protected override async Task DoApplyAsync(IAsyncDocumentSession session, CommandResult result)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var user = new User
            {
                Name = this.Name,
                Role = UserRole.Student
            };
            await session.StoreAsync(user).ConfigureAwait(false);
        }
    }
}