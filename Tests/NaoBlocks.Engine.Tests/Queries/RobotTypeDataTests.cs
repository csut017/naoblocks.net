using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Queries;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Queries
{
    public class RobotTypeDataTests : DatabaseHelper
    {
        [Fact]
        public async Task RetrieveByIdAsyncCallsDatabase()
        {
            using var store = InitialiseDatabase(new RobotType { Id = "users/1" });
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<RobotTypeData>(session);
            var result = await query.RetrieveByIdAsync("users/1");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task RetrieveByNameAsyncCallsDatabase()
        {
            using var store = InitialiseDatabase(new RobotType { Name = "Mihīni" });
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<RobotTypeData>(session);
            var result = await query.RetrieveByNameAsync("Mihīni");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task RetrieveDefaultAsyncCallsDatabase()
        {
            using var store = InitialiseDatabase(new RobotType { IsDefault = true });
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<RobotTypeData>(session);
            var result = await query.RetrieveDefaultAsync();
            Assert.NotNull(result);
        }

        [Fact]
        public async Task RetrievePageAsyncCallsDatabase()
        {
            using var store = InitialiseDatabase(new RobotType { Name = "Mihīni" });
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<RobotTypeData>(session);
            var result = await query.RetrievePageAsync(0, 10);
            Assert.Equal(1, result.Count);
            Assert.NotEmpty(result.Items ?? Array.Empty<RobotType>());
            Assert.Equal(0, result.Page);
        }
    }
}