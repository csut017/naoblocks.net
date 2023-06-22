﻿using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Newtonsoft.Json;
using Raven.Client.Documents;
using System.Text;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command for starting a user session via a token.
    /// </summary>
    public class StartUserSessionViaToken
        : CommandBase
    {
        /// <summary>
        /// Gets or sets the user role.
        /// </summary>
        /// <remarks>
        /// This property is needed for reloading as the token may be invalid in the database.
        /// </remarks>
        public UserRole Role { get; set; } = UserRole.Student;

        /// <summary>
        /// Gets or sets the token to use.
        /// </summary>
        [JsonIgnore]
        public string? Token { get; set; }

        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <remarks>
        /// This property is needed for reloading as the token may be invalid in the database.
        /// </remarks>
        public string? UserId { get; set; }

        /// <summary>
        /// Validates the user via the token.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        /// <param name="engine"></param>
        public override async Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.Token))
            {
                errors.Add(this.GenerateError($"Token is required"));
            }

            if (!errors.Any())
            {
                try
                {
                    var parts = this.Token!.Split(':');
                    var userName = Encoding.UTF8.GetString(Convert.FromBase64String(parts[0]));
                    var password = this.Token[(parts[0].Length + 1)..];
                    var user = await session.Query<User>()
                        .FirstOrDefaultAsync(u => u.Name == userName)
                        .ConfigureAwait(false);
                    if ((user == null) || (user.LoginToken?.Verify(password) != true))
                    {
                        errors.Add(this.GenerateError("Unknown or invalid user"));
                    }
                    else
                    {
                        this.UserId = user.Id;
                        this.Role = user.Role;
                    }
                }
                catch
                {
                    errors.Add(this.GenerateError("Unknown or invalid user"));
                }
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Stores the program in the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the command has not been validated.</exception>
        /// <param name="engine"></param>
        protected override async Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            ValidateExecutionState(this.UserId);
            var now = this.WhenExecuted;
            var userId = this.UserId!;
            var existing = await session.Query<Session>()
                .FirstOrDefaultAsync(us => us.UserId == userId && us.WhenExpires > now)
                .ConfigureAwait(false);
            if (existing == null)
            {
                var newSession = new Session
                {
                    Role = this.Role,
                    UserId = userId,
                    WhenAdded = now,
                    WhenExpires = now.AddDays(1)
                };
                await session.StoreAsync(newSession).ConfigureAwait(false);
                return CommandResult.New(this.Number, newSession);
            }

            existing.WhenExpires = now.AddDays(1);
            return CommandResult.New(this.Number, existing);
        }
    }
}