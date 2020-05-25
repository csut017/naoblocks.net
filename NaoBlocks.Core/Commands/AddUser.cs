using NaoBlocks.Core.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class AddUser
        : CommandBase<User>
    {
        public Password HashedPassword { get; set; } = Models.Password.Empty;

        public string? Name { get; set; }

        public string? Password { get; set; }

        public UserRole Role { get; set; }

        public UserSettings? Settings { get; set; }

        public async override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var errors = new List<CommandError>();
            var roleName = this.Role.ToString();

            if (this.Role == UserRole.Unknown)
            {
                roleName = "User";
                errors.Add(this.Error($"Role is unknown or missing"));
            }

            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add(this.Error($"{roleName} name is required"));
            }

            if (this.Password == null)
            {
                errors.Add(this.Error("Password is required"));
            }
            else
            {
                this.HashedPassword = Models.Password.New(this.Password);
                this.Password = null;
            }

            if (!errors.Any() && await session.Query<User>().AnyAsync(s => s.Name == this.Name).ConfigureAwait(false))
            {
                errors.Add(this.Error($"{roleName} with name {this.Name} already exists"));
            }

            return errors.AsEnumerable();
        }

        protected override async Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var user = new User
            {
                Name = this.Name ?? "<Unknown>",
                Role = this.Role,
                Password = this.HashedPassword,
                Settings = this.Settings ?? new UserSettings(),
                WhenAdded = this.WhenExecuted
            };
            await session.StoreAsync(user).ConfigureAwait(false);
            return this.Result(user);
        }
    }
}