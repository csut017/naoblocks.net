using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Newtonsoft.Json;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command for starting a user session via a username and password.
    /// </summary>
    public class StartUserSession
        : CommandBase
    {
        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        [JsonIgnore]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the password to check.
        /// </summary>
        [JsonIgnore]
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the user role.
        /// </summary>
        /// <remarks>
        /// This property is needed for reloading as the name and password are not stored.
        /// </remarks>
        public UserRole Role { get; set; } = UserRole.Student;

        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <remarks>
        /// This property is needed for reloading as the name and password are not stored.
        /// </remarks>
        public string? UserId { get; set; }

        /// <summary>
        /// Validates the user via their name and password.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        public async override Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add(this.GenerateError($"User name is required"));
            }

            if (string.IsNullOrWhiteSpace(this.Password))
            {
                errors.Add(this.GenerateError($"Password is required"));
            }

            if (!errors.Any())
            {
                var user = await session.Query<User>()
                    .FirstOrDefaultAsync(u => u.Name == this.Name)
                    .ConfigureAwait(false);
                if (user == null)
                {
                    errors.Add(this.GenerateError("Unknown or invalid user"));
                }
                else if ((user.Password != null) && !user.Password.Verify(this.Password))
                {
                    errors.Add(this.GenerateError("Unknown or invalid user"));
                }
                else
                {
                    this.UserId = user.Id;
                    this.Role = user.Role;
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
        protected async override Task<CommandResult> DoExecuteAsync(IDatabaseSession session)
        {
            this.ValidateExecutionState(this.UserId);
            var now = this.WhenExecuted;
            var userId = this.UserId ?? "<Unknown>";
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
