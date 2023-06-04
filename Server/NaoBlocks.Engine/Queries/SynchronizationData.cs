using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace NaoBlocks.Engine.Queries
{
    /// <summary>
    /// Queries for working with synchronization data.
    /// </summary>
    public class SynchronizationData
        : DataQuery
    {
        /// <summary>
        /// Retrieves a <see cref="SynchronizationSource"/> by it's name.
        /// </summary>
        /// <param name="name">The name of the source.</param>
        /// <returns>An <see cref="SynchronizationSource"/> if found, null otherwise.</returns>
        public virtual async Task<SynchronizationSource?> RetrieveSourceByNameAsync(string name)
        {
            var item = await this.Session.Query<SynchronizationSource>()
                .FirstOrDefaultAsync(s => s.Name == name);
            return item;
        }

        /// <summary>
        /// Retrieves a page of synchronization sources.
        /// </summary>
        /// <param name="pageNum">The page number to retrieve.</param>
        /// <param name="pageSize">The number of robots in the page.</param>
        /// <returns>A <see cref="ListResult{TData}"/> containing the sources.</returns>
        public virtual async Task<ListResult<SynchronizationSource>> RetrieveSourcePageAsync(int pageNum, int pageSize)
        {
            var query = ((IRavenQueryable<SynchronizationSource>)this.Session.Query<SynchronizationSource>())
                .Statistics(out QueryStatistics stats)
                .OrderBy(r => r.Name)
                .Skip(pageNum * pageSize)
                .Take(pageSize);

            var data = await query.ToListAsync();
            var result = new ListResult<SynchronizationSource>
            {
                Count = stats.TotalResults,
                Page = pageNum,
                Items = data
            };
            return result;
        }
    }
}