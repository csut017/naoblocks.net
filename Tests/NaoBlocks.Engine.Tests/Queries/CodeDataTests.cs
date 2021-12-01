using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Queries;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Queries
{
    public class CodeDataTests : DatabaseHelper
    {
        [Fact]
        public async Task RetrieveByNameAsyncCallsDatabase()
        {
            using var store = InitialiseDatabase(new CodeProgram { UserId = "Mia", Number = 1 });
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<CodeData>(session);
            var result = await query.RetrieveCodeAsync("Mia", 1);
            Assert.NotNull(result);
        }
    }
}
