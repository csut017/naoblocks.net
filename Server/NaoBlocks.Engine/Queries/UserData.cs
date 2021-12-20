using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

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

        /// <summary>
        /// Retrieve a user by their identifier.
        /// </summary>
        /// <param name="id">The user id.</param>
        /// <returns>The <see cref="User"/> instance if found, null otherwise.</returns>
        public virtual async Task<User?> RetrieveByIdAsync(string id)
        {
            var result = await this.Session.LoadAsync<User>(id)
                .ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Retrieves a page of users.
        /// </summary>
        /// <param name="pageNum">The page number to retrieve.</param>
        /// <param name="pageSize">The number of users in the page.</param>
        /// <returns>A <see cref="ListResult{TData}"/> containing the users.</returns>
        public virtual async Task<ListResult<User>> RetrievePageAsync(int pageNum, int pageSize)
        {
            var query = (IRavenQueryable<User>)this.Session.Query<User>();
            var users = await query.Statistics(out QueryStatistics stats)
                                    .OrderBy(s => s.Name)
                                    .Skip(pageNum * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();
            var result = new ListResult<User>
            {
                Count = stats.TotalResults,
                Page = pageNum,
                Items = users
            };
            return result;
        }

        /// <summary>
        /// Retrieves a page of users.
        /// </summary>
        /// <param name="pageNum">The page number to retrieve.</param>
        /// <param name="pageSize">The number of users in the page.</param>
        /// <returns>A <see cref="ListResult{TData}"/> containing the users.</returns>
        public virtual async Task<ListResult<User>> RetrievePageAsync(int pageNum, int pageSize, UserRole role)
        {
            var query = (IRavenQueryable<User>)this.Session.Query<User>();
            var users = await query.Statistics(out QueryStatistics stats)
                                    .OrderBy(s => s.Name)
                                    .Where(s => s.Role == role)
                                    .Skip(pageNum * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();
            var result = new ListResult<User>
            {
                Count = stats.TotalResults,
                Page = pageNum,
                Items = users
            };
            return result;
        }
    }
}
