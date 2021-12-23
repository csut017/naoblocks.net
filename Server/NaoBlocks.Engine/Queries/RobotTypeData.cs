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
    }
}
