using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// Abstract <see cref="CommandBase"/> that provides helper functionality for working with users.
    /// </summary>
    public abstract class UserCommandBase
        : CommandBase
    {
        /// <summary>
        /// Attempt to retrieve and validate a user.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <param name="name">The user's name.</param>
        /// <param name="role">The user's role.</param>
        /// <param name="errors">Where to append any validation errors.</param>
        /// <returns>The <see cref="User"/> instance, if valid, or null otherwise.</returns>
        public  async Task<User?> ValidateAndRetrieveUser(IDatabaseSession session, string? name, UserRole? role, List<CommandError> errors)
        {
            var roleName = role?.ToString() ?? "User";
            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add(this.GenerateError($"{roleName} name is required"));
            }

            User? person = null;
            if (!errors.Any())
            {
                person = role == null
                    ? await session.Query<User>()
                                .FirstOrDefaultAsync(u => u.Name == name)
                                .ConfigureAwait(false)
                    : await session.Query<User>()
                                .FirstOrDefaultAsync(u => u.Name == name && u.Role == role)
                                .ConfigureAwait(false);
                if (person == null) errors.Add(this.GenerateError($"{roleName} {name} does not exist"));
            }

            return person;
        }
    }
}