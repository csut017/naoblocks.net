using NaoBlocks.Core.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class UpdateUserCommand
        : CommandBase
    {
        private User? person;

        public string? CurrentName { get; set; }

        public string? Name { get; set; }

        public UserRole Role { get; set; }

        public async override Task<IEnumerable<string>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<string>();
            this.person = await session.Query<User>()
                                        .FirstOrDefaultAsync(u => u.Name == this.CurrentName)
                                        .ConfigureAwait(false);
            if (this.person == null) errors.Add($"{this.Role} {this.Name} does not exist");

            return errors.AsEnumerable();
        }

        protected override Task DoApplyAsync(IAsyncDocumentSession? session, CommandResult? result)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (this.person == null) throw new InvalidOperationException("ValidateAsync must be called first");
            if (!string.IsNullOrEmpty(this.Name) && (this.Name != this.person.Name)) this.person.Name = this.Name;
            return Task.CompletedTask;
        }
    }
}