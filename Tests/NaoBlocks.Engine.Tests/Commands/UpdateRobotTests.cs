using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using Raven.TestDriver;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class UpdateRobotTests : RavenTestDriver
    {
        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new UpdateRobot();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Machine name is required" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new UpdateRobot
            {
                MachineName = "mihīni"
            };
            using var store = GetDocumentStore();
            using (var initSession = store.OpenSession())
            {
                initSession.Store(new Robot { MachineName = "mihīni" });
                initSession.SaveChanges();
            }
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateHashesPassword()
        {
            var command = new UpdateRobot
            {
                Password = "1234"
            };
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            await engine.ValidateAsync(command);
            Assert.Null(command.Password);
            Assert.NotNull(command.HashedPassword);
            Assert.True(command.HashedPassword!.Verify("1234"));
        }

        [Fact]
        public async Task ValidateChecksForExistingRobot()
        {
            var command = new UpdateRobot
            {
                MachineName = "mihīni"
            };
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Robot mihīni does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreFailsIfRobotIsMissing()
        {
            var command = new UpdateRobot
            {
                MachineName = "mihīni"
            };
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Equal(new[] { "Robot mihīni does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreReloadsRobot()
        {
            var command = new UpdateRobot
            {
                MachineName = "mihīni"
            };
            using var store = GetDocumentStore();
            using (var initSession = store.OpenSession())
            {
                initSession.Store(new Robot { MachineName = "mihīni" });
                initSession.SaveChanges();
            }

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ExecuteSavesRobot()
        {
            var command = new UpdateRobot
            {
                MachineName = "mihīni",
                FriendlyName = "Whero",
                HashedPassword = Password.New("7890")
            };
            using var store = GetDocumentStore();
            using (var initSession = store.OpenSession())
            {
                initSession.Store(new Robot { MachineName = "mihīni", FriendlyName = "Whero Iti", Password = Password.New("1234") });
                initSession.SaveChanges();
            }

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var robot = verifySession.Query<Robot>().First();
            Assert.Equal("Whero", robot.FriendlyName);
            Assert.True(robot.Password.Verify("7890"));
        }

        [Fact]
        public async Task ExecuteChecksInitialState()
        {
            var command = new UpdateRobot
            {
                MachineName = "mihīni"
            };
            var engine = new FakeEngine();
            var result = await engine.ExecuteAsync(command);
            Assert.False(result.WasSuccessful);
            Assert.Equal("Unexpected error: Command is not in a valid state. Need to call either ValidateAsync or RestoreAsync", result.Error);
        }
    }
}
