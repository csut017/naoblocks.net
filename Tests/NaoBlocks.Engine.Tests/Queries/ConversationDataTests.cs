using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Indices;
using NaoBlocks.Engine.Queries;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Queries
{
    public class ConversationDataTests : DatabaseHelper
    {
        [Fact]
        public async Task RetrieveByIdAsyncCallsDatabase()
        {
            using var store = InitialiseDatabase(new Conversation { ConversationId = 1 });
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<ConversationData>(session);
            var result = await query.RetrieveByIdAsync(1);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task RetrieveRobotLogAsyncCallsDatabase()
        {
            var conversation = new Conversation { ConversationId = 4 };
            var now = DateTime.Now;
            using var store = InitialiseDatabase(
                conversation,
                new Robot { MachineName = "Mihīni", Id = "robots/3" },
                new RobotLog { RobotId = "robots/3", Conversation = conversation, WhenAdded = now });
            await store.ExecuteIndexAsync(new RobotLogByMachineName());
            WaitForIndexing(store);
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<ConversationData>(session);
            var result = await query.RetrieveRobotLogAsync(4, "Mihīni");
            Assert.Equal("robots/3", result?.RobotId);
            Assert.Equal(4, result?.Conversation.ConversationId);
            Assert.Equal(now, result?.WhenAdded);
        }
    }
}
