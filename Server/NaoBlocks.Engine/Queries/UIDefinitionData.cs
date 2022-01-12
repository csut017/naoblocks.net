using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

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
        /// <returns>The <see cref="UIDefinition"/> instance if found, null otherwise.</returns>
        public virtual async Task<UIDefinition?> RetrieveByNameAsync(string name)
        {
            var result = await this.Session
                .Query<UIDefinition>()
                .FirstOrDefaultAsync(d => d.Name == name);
            return result;
        }
    }
}
