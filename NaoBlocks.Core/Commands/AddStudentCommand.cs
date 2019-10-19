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

        public string Password { get; set; }

        public async override Task<IEnumerable<string>> ValidateAsync(IAsyncDocumentSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add("Student name is required");
            }

            if (this.Password == null)
            {
                errors.Add("Password is required");
            }

            if (!errors.Any() && await session.Query<User>().AnyAsync(s => s.Name == this.Name && s.Role == UserRole.Student).ConfigureAwait(false))
            {
                errors.Add($"Student with name {this.Name} already exists");
            }

            return errors.AsEnumerable();
        }

        protected override async Task DoApplyAsync(IAsyncDocumentSession session, CommandResult result)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var user = new User
            {
                Name = this.Name,
                Role = UserRole.Student,
                Password = Models.Password.New(this.Password)
            };
            await session.StoreAsync(user).ConfigureAwait(false);
        }
    }
}