using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Queries;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Queries
{
    public class SessionDataTests : DatabaseHelper
    {
        [Fact]
        public async Task RetrieveByIdAsyncCallsDatabase()
        {
            using var store = InitialiseDatabase(new Session { Id = "sessions/1" });
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<SessionData>(session);
            var result = await query.RetrieveByIdAsync("sessions/1");
            Assert.NotNull(result);
        }
    }
}
