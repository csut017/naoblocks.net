using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Queries;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Queries
{
    public class RobotTypeDataTests : DatabaseHelper
    {
        [Fact]
        public async Task RetrieveByNameAsyncCallsDatabase()
        {
            using var store = InitialiseDatabase(new RobotType { Name = "Mihīni" });
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<RobotTypeData>(session);
            var result = await query.RetrieveByNameAsync("Mihīni");
            Assert.NotNull(result);
        }
    }
}
