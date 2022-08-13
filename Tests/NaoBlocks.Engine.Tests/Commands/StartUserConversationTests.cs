using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class StartUserConversationTests : DatabaseHelper
    {
        [Fact]
        public async Task ExecuteAddsSystemValues()
        {
            var command = new StartUserConversation
            {
                Name = "Bob",
                Type = ConversationType.Program,
            };

            using var store = InitialiseDatabase(new User { Name = "Bob" });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);

                Assert.True(result.WasSuccessful);
                Assert.True(engine.DatabaseSession.StoreCalled);
                var values = Assert.IsType<Conversation>(engine.DatabaseSession.GetLastModifiedEntity());
                Assert.Equal(1, values.ConversationId);
                Assert.Equal(ConversationType.Program, values.ConversationType);
                Assert.Equal(1, result?.As<Conversation>()?.Output?.ConversationId);
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var record = verifySession.Query<SystemValues>().FirstOrDefault();
            Assert.Equal(1, record?.NextConversationId);
        }

        [Fact]
        public async Task ExecuteChecksInitialState()
        {
            var command = new StartUserConversation
            {
                Name = " Bob "
            };
            var engine = new FakeEngine();
            var result = await engine.ExecuteAsync(command);
            Assert.False(result.WasSuccessful);
            Assert.Equal("Unexpected error: Command is not in a valid state. Need to call either ValidateAsync or RestoreAsync", result.Error);
        }

        [Fact]
        public async Task ExecuteUpdatesSystemValues()
        {
            var command = new StartUserConversation
            {
                Name = "Bob"
            };

            using var store = InitialiseDatabase(
                new User { Name = "Bob" },
                new SystemValues { NextConversationId = 10 });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);

                Assert.True(result.WasSuccessful);
                Assert.True(engine.DatabaseSession.StoreCalled);
                var values = Assert.IsType<Conversation>(engine.DatabaseSession.GetLastModifiedEntity());
                Assert.Equal(11, values.ConversationId);
                Assert.Equal(ConversationType.Unknown, values.ConversationType);
                Assert.Equal(11, result?.As<Conversation>()?.Output?.ConversationId);
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var record = verifySession.Query<SystemValues>().FirstOrDefault();
            Assert.Equal(11, record?.NextConversationId);
        }

        [Fact]
        public async Task RestoreFailsIfUserIsMissing()
        {
            var command = new StartUserConversation
            {
                Name = "Bob"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Equal(new[] { "User Bob does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreReloadsUser()
        {
            var command = new StartUserConversation
            {
                Name = "Bob"
            };
            using var store = InitialiseDatabase(new User { Name = "Bob", Role = UserRole.Student });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateChecksForExistingUser()
        {
            var command = new StartUserConversation
            {
                Name = "Bob"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "User Bob does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new StartUserConversation
            {
                Name = "Bob"
            };
            using var store = InitialiseDatabase(new User { Name = "Bob", Role = UserRole.Student });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new StartUserConversation();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Name is required" }, FakeEngine.GetErrors(errors));
        }
    }
}