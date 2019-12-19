using NaoBlocks.Core.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class AddUserCommand
        : CommandBase<User>
    {
        public Password HashedPassword { get; set; } = Models.Password.Empty;

        public string? Name { get; set; }

        public string? Password { get; set; }

        public UserRole Role { get; set; }

        public async override Task<IEnumerable<string>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add($"{this.Role} name is required");
            }

            if (this.Password == null)
            {
                errors.Add("Password is required");
            }
            else
            {
                this.HashedPassword = Models.Password.New(this.Password);
                this.Password = null;
            }

            if (!errors.Any() && await session.Query<User>().AnyAsync(s => s.Name == this.Name && s.Role == this.Role).ConfigureAwait(false))
            {
                errors.Add($"{this.Role} with name {this.Name} already exists");
            }

            return errors.AsEnumerable();
        }

        protected override async Task DoApplyAsync(IAsyncDocumentSession? session, CommandResult? result)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var user = new User
            {
                Name = this.Name ?? "<Unknown>",
                Role = this.Role,
                Password = this.HashedPassword,
                WhenAdded = this.WhenExecuted
            };
            await session.StoreAsync(user).ConfigureAwait(false);
            this.Output = user;
        }
    }
}