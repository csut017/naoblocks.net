using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace NaoBlocks.Engine.Queries
{
    /// <summary>
    /// Queries for accessing robot data.
    /// </summary>
    public class RobotData
        : DataQuery
    {
        /// <summary>
        /// Retrieve a robot by their machine name.
        /// </summary>
        /// <param name="name">The robot's mchine name.</param>
        /// <param name="loadTypeDetails">Whether to include the type details or not.</param>
        /// <returns>The <see cref="Robot"/> instance.</returns>
        public virtual async Task<Robot?> RetrieveByNameAsync(string name, bool loadTypeDetails = false)
        {
            var result = await this.Session.Query<Robot>()
                .FirstOrDefaultAsync(r => r.MachineName == name)
                .ConfigureAwait(false);

            if (loadTypeDetails && (result != null))
            {
                result.Type = await this.Session.LoadAsync<RobotType>(result.RobotTypeId)
                    .ConfigureAwait(false);
            }
            return result;
        }

        /// <summary>
        /// Retrieve a robot by their identifier.
        /// </summary>
        /// <param name="id">The robot id.</param>
        /// <returns>The <see cref="Robot"/> instance if found, null otherwise.</returns>
        public virtual async Task<Robot?> RetrieveByIdAsync(string id)
        {
            var result = await this.Session
                .LoadAsync<Robot>(id)
                .ConfigureAwait(false);
            if ((result != null) && !string.IsNullOrEmpty(result.RobotTypeId))
            {
                result.Type = await this.Session
                    .LoadAsync<RobotType>(result.RobotTypeId)
                    .ConfigureAwait(false);
            }
            return result;
        }

        /// <summary>
        /// Retrieves a page of robots.
        /// </summary>
        /// <param name="pageNum">The page number to retrieve.</param>
        /// <param name="pageSize">The number of robots in the page.</param>
        /// <param name="robotTypeId">An optional filter for the type of the robot.</param>
        /// <returns>A <see cref="ListResult{TData}"/> containing the robots.</returns>
        public virtual async Task<ListResult<Robot>> RetrievePageAsync(int pageNum, int pageSize, string? robotTypeId = null)
        {
            var query = ((IRavenQueryable<Robot>)this.Session.Query<Robot>())
                .Statistics(out QueryStatistics stats)
                .OrderBy(r => r.MachineName)
                .Skip(pageNum * pageSize)
                .Take(pageSize);
            if (robotTypeId != null)
            {
                query = query.Where(r => r.RobotTypeId == robotTypeId);
            }

            var data = await query.ToListAsync();
            var result = new ListResult<Robot>
            {
                Count = stats.TotalResults,
                Page = pageNum,
                Items = data
            };
            return result;
        }
    }
}
