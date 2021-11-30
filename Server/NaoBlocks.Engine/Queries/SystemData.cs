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
        public virtual async Task<SystemValues> RetrieveSystemValuesAsync()
        {
            var settings = await this.Session.Query<SystemValues>()
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
            return settings != null ? settings : new SystemValues();
        }
    }
}
