using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace NaoBlocks.Engine.Queries
{
    /// <summary>
    /// Queries for working with programs.
    /// </summary>
    public class CodeData
        : DataQuery
    {
        /// <summary>
        /// Retrieve a program by username and program id.
        /// </summary>
        /// <param name="user">The user's name.</param>
        /// <param name="program">The program number.</param>
        /// <returns>The <see cref="User"/> instance.</returns>
        public virtual async Task<CodeProgram?> RetrieveCodeAsync(string user, long program)
        {
            var result = await this.Session.Query<CodeProgram>()
                .FirstOrDefaultAsync(p => p.Number == program && p.UserId == user)
                .ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Retrieve all the programs for a user.
        /// </summary>
        /// <param name="user">The user's name.</param>
        /// <param name="pageNum">The page number to retrieve.</param>
        /// <param name="pageSize">The number of users in the page.</param>
        /// <param name="namedOnly">Whether to only include named programs or not.</param>
        /// <returns>The <see cref="ListResult{TData}"/> containing the <see cref="CodeProgram"/> instances.</returns>
        public virtual async Task<ListResult<CodeProgram>> RetrieveForUserAsync(string user, int pageNum, int pageSize, bool namedOnly = true)
        {
            var query = (IRavenQueryable<CodeProgram>)this.Session.Query<CodeProgram>();
            query = query.Statistics(out QueryStatistics stats)
                                    .OrderBy(p => p.Number)
                                    .Where(p => p.UserId == user)
                                    .Skip(pageNum * pageSize)
                                    .Take(pageSize);
            if (namedOnly) query = query.Where(p => p.Name != null);
            var programs = await query.ToListAsync();
            var result = new ListResult<CodeProgram>
            {
                Count = stats.TotalResults,
                Page = pageNum,
                Items = programs
            };
            return result;
        }
    }
}
