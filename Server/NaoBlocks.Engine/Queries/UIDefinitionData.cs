using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace NaoBlocks.Engine.Queries
{
    /// <summary>
    /// Queries for accessing UI definition data.
    /// </summary>
    public class UIDefinitionData
        : DataQuery
    {
        /// <summary>
        /// Retrieve a UI definition by its name.
        /// </summary>
        /// <param name="name">The name of the definition.</param>
        /// <returns>The <see cref="UIDefinition"/> instance if found, null otherwise.</returns>
        public virtual async Task<UIDefinition?> RetrieveByNameAsync(string name)
        {
            var result = await this.Session
                .Query<UIDefinition>()
                .FirstOrDefaultAsync(d => d.Name == name);
            return result;
        }

        /// <summary>
        /// Retrieve a page of UI definitions.
        /// </summary>
        /// <param name="pageNum">The page number to retrieve.</param>
        /// <param name="pageSize">The number of definitions in the page.</param>
        /// <returns>A <see cref="ListResult{TData}"/> containing the definitions.</returns>
        public virtual async Task<ListResult<UIDefinition>> RetrievePageAsync(int pageNum, int pageSize)
        {
            var query = (IRavenQueryable<UIDefinition>)this.Session.Query<UIDefinition>();
            var items = await query.Statistics(out QueryStatistics stats)
                                    .OrderBy(s => s.Name)
                                    .Skip(pageNum * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();
            var result = new ListResult<UIDefinition>
            {
                Count = stats.TotalResults,
                Page = pageNum,
                Items = items
            };
            return result;
        }
    }
}