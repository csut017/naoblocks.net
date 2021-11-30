using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Queries
{
    /// <summary>
    /// Queries for accessing user data.
    /// </summary>
    public class UserData
        : DataQuery
    {
        /// <summary>
        /// Checks if there are any users in the database.
        /// </summary>
        /// <returns>True if there is at least one yser</returns>
        public virtual async Task<bool> CheckForAnyAsync()
        {
            var hasAny = await this.Session.Query<User>()
                .AnyAsync()
                .ConfigureAwait(false);
            return hasAny;
        }

        /// <summary>
        /// Retrieve a user by their name.
        /// </summary>
        /// <param name="name">The user's name.</param>
        /// <returns>The <see cref="User"/> instance.</returns>
        public virtual async Task<User?> RetrieveByNameAsync(string name)
        {
            var result = await this.Session.Query<User>()
                .FirstOrDefaultAsync(u => u.Name == name)
                .ConfigureAwait(false);
            return result;
        }
    }
}
