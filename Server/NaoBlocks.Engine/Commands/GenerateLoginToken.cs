﻿using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using System.Security.Cryptography;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command for generating a login token.
    /// </summary>
    public class GenerateLoginToken
        : UserCommandBase
    {
        private User? person;

        /// <summary>
        /// Gets or sets the name of the person logging in.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets whether any existing session should be overriden or not.
        /// </summary>
        public bool? OverrideExisting { get; set; }

        /// <summary>
        /// Gets or sets the security token.
        /// </summary>
        public string SecurityToken { get; set; } = string.Empty;

        /// <summary>
        /// Attempts to restore the command from the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during restoration.</returns>
        public override async Task<IEnumerable<CommandError>> RestoreAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            this.person = await this.ValidateAndRetrieveUser(session, this.Name, UserRole.User, errors).ConfigureAwait(false);
            return errors.AsEnumerable();
        }

        /// <summary>
        /// Validates the user and generates the token.
        /// </summary>
        /// <param name="session">The database session.</param>
        /// <returns>Any valdiation errors.</returns>
        /// <param name="engine"></param>
        public override async Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var errors = new List<CommandError>();
            this.person = await this.ValidateAndRetrieveUser(session, this.Name, UserRole.User, errors);

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

        /// <summary>
        /// Sets the session token.
        /// </summary>
        /// <param name="session">The database session.</param>
        /// <returns>A <see cref="CommandResult"/> containing the asbtract syntax tree.</returns>
        /// <param name="engine"></param>
        protected override Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            ValidateExecutionState(this.person);
            this.person!.LoginToken = this.SecurityToken;
            var result = CommandResult.New(this.Number, this.person);
            return Task.FromResult(result);
        }
    }
}