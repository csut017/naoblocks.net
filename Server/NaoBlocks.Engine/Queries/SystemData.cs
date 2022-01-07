using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Queries
{
    /// <summary>
    /// Queries for accessing system data.
    /// </summary>
    public class SystemData
        : DataQuery
    {
        /// <summary>
        /// Attempts to retrieve the system values.
        /// </summary>
        /// <returns>A <see cref="SystemValues"/> instance containing the system values (will be empty if not found.)</returns>
        public virtual async Task<SystemValues> RetrieveSystemValuesAsync()
        {
            var settings = await this.Session.Query<SystemValues>()
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
            return settings ?? new SystemValues();
        }
    }
}
