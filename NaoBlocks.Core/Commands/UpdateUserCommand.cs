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

        public Password? HashedPassword { get; set; }

        public string? Name { get; set; }

        public string? Password { get; set; }

        public UserRole Role { get; set; }

        public UserSettings? Settings { get; set; }

        public async override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<CommandError>();
            this.person = await session.Query<User>()
                                        .FirstOrDefaultAsync(u => u.Name == this.CurrentName)
                                        .ConfigureAwait(false);
            if (this.person == null) errors.Add(this.Error($"{this.Role} {this.CurrentName} does not exist"));

            if (this.Password != null)
            {
                this.HashedPassword = Models.Password.New(this.Password);
                this.Password = null;
            }

            return errors.AsEnumerable();
        }

        protected override Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (this.person == null) throw new InvalidOperationException("ValidateAsync must be called first");
            if (!string.IsNullOrEmpty(this.Name) && (this.Name != this.person.Name)) this.person.Name = this.Name;
            if (this.HashedPassword != null) this.person.Password = this.HashedPassword;
            if (this.Settings != null) this.person.Settings = this.Settings;
            return Task.FromResult(CommandResult.New(this.Number));
        }
    }
}