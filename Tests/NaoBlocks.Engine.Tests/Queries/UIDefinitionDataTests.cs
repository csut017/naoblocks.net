using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Queries;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Queries
{
    public class UIDefinitionDataTests : DatabaseHelper
    {
        [Fact]
        public async Task RetrieveByNameAsyncCallsDatabase()
        {
            using var store = InitialiseDatabase(new UIDefinition { Name = "angular" });
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<UIDefinitionData>(session);
            var result = await query.RetrieveByNameAsync("angular");
            Assert.NotNull(result);
        }
    }
}
