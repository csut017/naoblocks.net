using NaoBlocks.Engine.Data;

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
    }
}
