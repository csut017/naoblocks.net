using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Queries;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Queries
{
    public class SystemDataTests : DatabaseHelper
    {
        [Fact]
        public async Task RetrieveSystemValuesCallsDatabase()
        {
            using var store = InitialiseDatabase(new SystemValues { DefaultAddress = "123" });
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<SystemData>(session);
            var result = await query.RetrieveSystemValuesAsync();
            Assert.Equal("123", result.DefaultAddress);
        }

        [Fact]
        public async Task RetrieveSystemValuesHandlesMissingRecord()
        {
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<SystemData>(session);
            var result = await query.RetrieveSystemValuesAsync();
            Assert.Equal(string.Empty, result.DefaultAddress);
        }
    }
}
