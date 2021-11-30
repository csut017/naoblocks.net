using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Queries;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Queries
{
    public class UserDataTests : DatabaseHelper
    {
        [Fact]
        public async Task CheckForAnyCallsDatabase()
        {
            using var store = InitialiseDatabase(new User { Name = "Mia", Role = UserRole.Student });
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<UserData>(session);
            var result = await query.CheckForAnyAsync();
            Assert.True(result);
        }

        [Fact]
        public async Task RetrieveByNameAsyncCallsDatabase()
        {
            using var store = InitialiseDatabase(new User { Name = "Mia", Role = UserRole.Student });
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<UserData>(session);
            var result = await query.RetrieveByNameAsync("Mia");
            Assert.NotNull(result);
        }
    }
}
