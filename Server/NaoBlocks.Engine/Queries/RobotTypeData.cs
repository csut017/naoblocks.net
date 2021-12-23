using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace NaoBlocks.Engine.Queries
{
    /// <summary>
    /// Queries for accessing robot type data.
    /// </summary>
    public class RobotTypeData
        : DataQuery
    {
        /// <summary>
        /// Retrieve a robot type by its name.
        /// </summary>
        /// <param name="name">The robot type's name.</param>
        /// <returns>The <see cref="RobotType"/> instance.</returns>
        public virtual async Task<RobotType?> RetrieveByNameAsync(string name)
        {
            var result = await this.Session.Query<RobotType>()
                .FirstOrDefaultAsync(r => r.Name == name)
                .ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Retrieves a page of robot types.
        /// </summary>
        /// <param name="pageNum">The page number to retrieve.</param>
        /// <param name="pageSize">The number of robot types in the page.</param>
        /// <returns>A <see cref="ListResult{TData}"/> containing the robot types.</returns>
        public virtual async Task<ListResult<RobotType>> RetrievePageAsync(int pageNum, int pageSize)
        {
            var query = ((IRavenQueryable<RobotType>)this.Session.Query<RobotType>())
                .Statistics(out QueryStatistics stats)
                .OrderBy(r => r.Name)
                .Skip(pageNum * pageSize)
                .Take(pageSize);
            var data = await query.ToListAsync();
            var result = new ListResult<RobotType>
            {
                Count = stats.TotalResults,
                Page = pageNum,
                Items = data
            };
            return result;
        }
    }
}
