using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Queries
{
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
    }
}
