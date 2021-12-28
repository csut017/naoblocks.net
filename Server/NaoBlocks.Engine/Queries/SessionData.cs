using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Queries
{
    /// <summary>
    /// Queries for accessing session data.
    /// </summary>
    public class SessionData
        : DataQuery
    {
        /// <summary>
        /// Retrieve a session by their identifier.
        /// </summary>
        /// <param name="id">The session id.</param>
        /// <returns>The <see cref="Session"/> instance if found, null otherwise.</returns>
        public virtual async Task<Session?> RetrieveByIdAsync(string id)
        {
            var result = await this.Session.LoadAsync<Session>(id)
                .ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Retrieve a session for a user.
        /// </summary>
        /// <param name="user">The user to retrieve the session for.</param>
        /// <returns>The <see cref="Session"/> instance if found, null otherwise.</returns>
        public virtual async Task<Session?> RetrieveForUserAsync(User user)
        {
            var result = await this.Session
                .Query<Session>()
                .FirstOrDefaultAsync(s => s.UserId == user.Id)
                .ConfigureAwait(false);
            return result;
        }
    }
}