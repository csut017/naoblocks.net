using NaoBlocks.Core.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class DeleteStudentCommand
        : CommandBase
    {
        private User student;
        public string Name { get; set; }

        public async override Task<IEnumerable<string>> ValidateAsync(IAsyncDocumentSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add("Student name is required");
            }

            if (!errors.Any())
            {
                this.student = await session.Query<User>()
                                            .FirstOrDefaultAsync(u => u.Name == this.Name && u.Role == UserRole.Student)
                                            .ConfigureAwait(false);
                if (student == null) errors.Add($"Student {this.Name} does not exist");
            }

            return errors.AsEnumerable();
        }

        protected override Task DoApplyAsync(IAsyncDocumentSession session, CommandResult result)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            session.Delete(this.student);
            return Task.CompletedTask;
        }
    }
}