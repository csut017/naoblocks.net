using NaoBlocks.Common;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class AddToRobotLogTests : DatabaseHelper
    {
        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new AddToRobotLog();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Machine name is required", "Description is required" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new AddToRobotLog
            {
                MachineName = "Mihīni",
                Description = "The description",
                ConversationId = 14916
            };
            using var store = InitialiseDatabase(
                new Robot { MachineName = "Mihīni" },
                new Conversation { ConversationId = 14916 });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateChecksForExistingRobot()
        {
            var command = new AddToRobotLog
            {
                MachineName = "Mihīni",
                Description = "The description",
                ConversationId = 14916
            };
            using var store = InitialiseDatabase(
                new Conversation { ConversationId = 14916 });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Robot Mihīni does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateChecksForExistingConversation()
        {
            var command = new AddToRobotLog
            {
                MachineName = "Mihīni",
                Description = "The description",
                ConversationId = 14916
            };
            using var store = InitialiseDatabase(
                new Robot { MachineName = "Mihīni" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Unknown conversation" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreFailsIfRobotIsMissing()
        {
            var command = new AddToRobotLog
            {
                MachineName = "Mihīni",
                ConversationId = 14916
            };
            using var store = InitialiseDatabase(
                new Conversation { ConversationId = 14916 });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Equal(new[] { "Robot Mihīni does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreFailsIfConversationIsMissing()
        {
            var command = new AddToRobotLog
            {
                MachineName = "Mihīni",
                ConversationId = 14916
            };
            using var store = InitialiseDatabase(
                new Robot { MachineName = "Mihīni" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Equal(new[] { "Unknown conversation" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreReloadsRobotAndConversation()
        {
            var command = new AddToRobotLog
            {
                MachineName = "Mihīni",
                ConversationId = 14916
            };
            using var store = InitialiseDatabase(
                new Robot { MachineName = "Mihīni" },
                new Conversation { ConversationId = 14916 });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ExecuteAddsNewLog()
        {
            var command = new AddToRobotLog
            {
                MachineName = "Mihīni",
                Description = "The description",
                ConversationId = 14916,
                SourceMessageType = ClientMessageType.Authenticate
            };
            using var store = InitialiseDatabase(
                new Robot { MachineName = "Mihīni", Id = "tahi" },
                new Conversation { ConversationId = 14916 });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var log = verifySession.Query<RobotLog>().First();
            Assert.Equal("The description", log.Lines.First().Description);
            Assert.Equal(ClientMessageType.Authenticate, log.Lines.First().SourceMessageType);
            Assert.Equal(14916, log.Conversation.ConversationId);
            Assert.Equal("tahi", log.RobotId);
        }

        [Fact]
        public async Task ExecuteUpdatesExistingLog()
        {
            var command = new AddToRobotLog
            {
                MachineName = "Mihīni",
                Description = "The description",
                ConversationId = 14916,
                SourceMessageType = ClientMessageType.Authenticate
            };
            using var store = InitialiseDatabase(
                new Robot { MachineName = "Mihīni", Id = "tahi" },
                new Conversation { ConversationId = 14916 },
                new RobotLog { RobotId = "tahi", Conversation = new Conversation { ConversationId = 14916 } });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var log = verifySession.Query<RobotLog>().First();
            Assert.Equal("The description", log.Lines.First().Description);
            Assert.Equal(ClientMessageType.Authenticate, log.Lines.First().SourceMessageType);
            Assert.Equal(14916, log.Conversation.ConversationId);
            Assert.Equal("tahi", log.RobotId);
        }

        [Fact]
        public async Task ExecuteUpdatesCopiesValues()
        {
            var command = new AddToRobotLog
            {
                MachineName = "Mihīni",
                Description = "The description",
                ConversationId = 14916,
                SourceMessageType = ClientMessageType.Authenticate
            };
            command.Values.Add(new NamedValue { Name = "rua", Value = "toru" });
            using var store = InitialiseDatabase(
                new Robot { MachineName = "Mihīni", Id = "tahi" },
                new Conversation { ConversationId = 14916 },
                new RobotLog { RobotId = "tahi", Conversation = new Conversation { ConversationId = 14916 } });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var log = verifySession.Query<RobotLog>().First();
            Assert.NotEmpty(log.Lines.First().Values);
        }

        [Fact]
        public async Task ExecuteChecksInitialStateOfRobot()
        {
            var command = new AddToRobotLog
            {
                ConversationId = 14916
            };
            using var store = InitialiseDatabase(
                new Robot { MachineName = "Mihīni" },
                new Conversation { ConversationId = 14916 });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            await engine.RestoreAsync(command);
            var result = await engine.ExecuteAsync(command);
            Assert.False(result.WasSuccessful);
            Assert.Equal("Unexpected error: Command is not in a valid state. Need to call either ValidateAsync or RestoreAsync", result.Error);
        }

        [Fact]
        public async Task ExecuteChecksInitialStateOfConversation()
        {
            var command = new AddToRobotLog
            {
                MachineName = "Mihīni",
                ConversationId = 14916
            };
            using var store = InitialiseDatabase(
                new Robot { MachineName = "Mihīni" });
            var engine = new FakeEngine();
            var result = await engine.ExecuteAsync(command);
            Assert.False(result.WasSuccessful);
            Assert.Equal("Unexpected error: Command is not in a valid state. Need to call either ValidateAsync or RestoreAsync", result.Error);
        }
    }
}
