using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Queries;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Queries
{
    public class RobotDataTests : DatabaseHelper
    {
        [Fact]
        public async Task RetrieveByNameAsyncCallsDatabase()
        {
            using var store = InitialiseDatabase(new Robot { MachineName = "Mihīni" });
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<RobotData>(session);
            var result = await query.RetrieveByNameAsync("Mihīni");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task RetrieveByIdAsyncCallsDatabase()
        {
            using var store = InitialiseDatabase(new Robot { Id = "robots/1" });
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<RobotData>(session);
            var result = await query.RetrieveByIdAsync("robots/1");
            Assert.NotNull(result);
            Assert.Null(result?.Type);
        }

        [Fact]
        public async Task RetrieveByIdAsyncHandlesMissingRobot()
        {
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<RobotData>(session);
            var result = await query.RetrieveByIdAsync("robots/1");
            Assert.Null(result);
        }

        [Fact]
        public async Task RetrieveByIdAsyncLoadsRobotType()
        {
            using var store = InitialiseDatabase(
                new RobotType { Id = "robotTypes/1" },
                new Robot { RobotTypeId = "robotTypes/1", Id = "robots/1" });
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<RobotData>(session);
            var result = await query.RetrieveByIdAsync("robots/1");
            Assert.NotNull(result);
            Assert.NotNull(result?.Type);
        }

        [Fact]
        public async Task RetrievePageAsyncCallsDatabase()
        {
            using var store = InitialiseDatabase(new Robot { MachineName = "Mihīni" });
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<RobotData>(session);
            var result = await query.RetrievePageAsync(0, 10);
            Assert.Equal(1, result.Count);
            Assert.NotEmpty(result.Items);
            Assert.Equal(0, result.Page);
        }
    }
}
