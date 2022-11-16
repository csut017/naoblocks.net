using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Queries;
using System.Linq;
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

        [Fact]
        public async Task RetrievePageAsyncCallsDatabase()
        {
            using var store = InitialiseDatabase(
                new UIDefinition { Name = "angular" },
                new UIDefinition { Name = "default" },
                new UIDefinition { Name = "local" },
                new UIDefinition { Name = "react" },
                new UIDefinition { Name = "tangible" });
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<UIDefinitionData>(session);
            var result = await query.RetrievePageAsync(1, 2);
            Assert.Equal(5, result.Count);
            Assert.Equal(1, result.Page);
            Assert.Equal(
                new[] { "local", "react" },
                result.Items?.Select(i => i.Name).ToArray());
        }
    }
}