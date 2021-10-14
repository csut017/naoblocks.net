using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using NaoBlocks.Common;
using NaoBlocks.Core.Commands.Helpers;
using NaoBlocks.Core.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class GenerateLoginToken
        : CommandBase<User>
    {
        private User? person;

        public string? Name { get; set; }

        public bool? OverrideExisting { get; set; }

        public string SecurityToken { get; set; } = string.Empty;

        public async override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<CommandError>();
            this.person = await session.Query<User>()
                                        .FirstOrDefaultAsync(u => u.Name == this.Name)
                                        .ConfigureAwait(false);
            if (this.person == null) errors.Add(this.GenerateError($"User {this.Name} does not exist"));

            this.OverrideExisting ??= false;
            if (!errors.Any() && (this.person != null))
            {
                if (!this.OverrideExisting.Value && !string.IsNullOrEmpty(this.person.LoginToken))
                {
                    this.SecurityToken = this.person.LoginToken;
                }
                else
                {
                    var baseToken = this.person.Name + Guid.NewGuid().ToString("N");
                    byte[] salt = new byte[128 / 8];
                    using (var rng = RandomNumberGenerator.Create())
                    {
                        rng.GetBytes(salt);
                    }
                    this.SecurityToken = Convert.ToBase64String(
                            KeyDerivation.Pbkdf2(baseToken, salt, KeyDerivationPrf.HMACSHA256, 10000, 256 / 8));
                }
            }

            return errors.AsEnumerable();
        }

        protected override async Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if ((this.person == null) || string.IsNullOrEmpty(this.SecurityToken)) throw new InvalidOperationException("ValidateAsync must be called first");
            this.person.LoginToken = this.SecurityToken;
            return this.Result(this.person);
        }
    }
}
