using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class StartRobotConversationTests : DatabaseHelper
    {
        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new StartRobotConversation();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Name is required" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new StartRobotConversation
            {
                Name = "Karetao"
            };
            using var store = InitialiseDatabase(new Robot { MachineName = "Karetao" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateChecksForExistingRobot()
        {
            var command = new StartRobotConversation
            {
                Name = "Karetao"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Robot Karetao does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ExecuteAddsSystemValues()
        {
            var command = new StartRobotConversation
            {
                Name = "Karetao"
            };

            using var store = InitialiseDatabase(new Robot { MachineName = "Karetao" });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);

                Assert.True(result.WasSuccessful);
                Assert.True(engine.DatabaseSession.StoreCalled);
                var values = Assert.IsType<Conversation>(engine.DatabaseSession.GetLastModifiedEntity());
                Assert.Equal(1, values.ConversationId);
                Assert.Equal(1, result?.As<Conversation>()?.Output?.ConversationId);
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var record = verifySession.Query<SystemValues>().FirstOrDefault();
            Assert.Equal(1, record?.NextConversationId);
        }

        [Fact]
        public async Task ExecuteUpdatesSystemValues()
        {
            var command = new StartRobotConversation
            {
                Name = "Karetao"
            };

            using var store = InitialiseDatabase(
                new Robot { MachineName = "Karetao" },
                new SystemValues {  NextConversationId = 10 });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);

                Assert.True(result.WasSuccessful);
                Assert.True(engine.DatabaseSession.StoreCalled);
                var values = Assert.IsType<Conversation>(engine.DatabaseSession.GetLastModifiedEntity());
                Assert.Equal(11, values.ConversationId);
                Assert.Equal(11, result?.As<Conversation>()?.Output?.ConversationId);
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var record = verifySession.Query<SystemValues>().FirstOrDefault();
            Assert.Equal(11, record?.NextConversationId);
        }

        [Fact]
        public async Task ExecuteChecksInitialState()
        {
            var command = new StartRobotConversation
            {
                Name = " Karetao "
            };
            var engine = new FakeEngine();
            var result = await engine.ExecuteAsync(command);
            Assert.False(result.WasSuccessful);
            Assert.Equal("Unexpected error: Command is not in a valid state. Need to call either ValidateAsync or RestoreAsync", result.Error);
        }

        [Fact]
        public async Task RestoreFailsIfRobotIsMissing()
        {
            var command = new StartRobotConversation
            {
                Name = "Karetao"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Equal(new[] { "Robot Karetao does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreReloadsRobot()
        {
            var command = new StartRobotConversation
            {
                Name = "Karetao"
            };
            using var store = InitialiseDatabase(new Robot { MachineName = "Karetao" });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Empty(errors);
        }
    }
}
