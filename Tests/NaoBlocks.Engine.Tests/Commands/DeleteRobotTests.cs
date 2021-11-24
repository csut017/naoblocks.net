using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using Raven.TestDriver;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class DeleteRobotTests : RavenTestDriver
    {
        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new DeleteRobot();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Robot name is required" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new DeleteRobot
            {
                Name = "Whero Iti"
            };
            using var store = GetDocumentStore();
            using (var initSession = store.OpenSession())
            {
                initSession.Store(new Robot { MachineName = "Whero Iti" });
                initSession.SaveChanges();
            }
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateChecksForExistingRobot()
        {
            var command = new DeleteRobot
            {
                Name = "Whero Iti"
            };
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Robot Whero Iti does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ExecuteDeletesRobot()
        {
            var command = new DeleteRobot
            {
                Name = "Whero Iti",
            };

            using var store = GetDocumentStore();
            using (var initSession = store.OpenSession())
            {
                initSession.Store(new Robot { MachineName = "Whero Iti" });
                initSession.SaveChanges();
            }

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            await engine.RestoreAsync(command);
            var result = await engine.ExecuteAsync(command);
            Assert.True(result.WasSuccessful);
            Assert.True(engine.DatabaseSession.DeletedCalled);
            Assert.NotNull(engine.DatabaseSession.GetLastModifiedEntity());
        }

        [Fact]
        public async Task ExecuteChecksInitialState()
        {
            var command = new DeleteRobot
            {
                Name = " Bob ",
            };
            var engine = new FakeEngine();
            var result = await engine.ExecuteAsync(command);
            Assert.False(result.WasSuccessful);
            Assert.Equal("Unexpected error: Command is not in a valid state. Need to call either ValidateAsync or RestoreAsync", result.Error);
        }

        [Fact]
        public async Task RestoreFailsIfRobotIsMissing()
        {
            var command = new DeleteRobot
            {
                Name = "Whero Iti"
            };
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Equal(new[] { "Robot Whero Iti does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreReloadsRobot()
        {
            var command = new DeleteRobot
            {
                Name = "Whero Iti"
            };
            using var store = GetDocumentStore();
            using (var initSession = store.OpenSession())
            {
                initSession.Store(new Robot { MachineName = "Whero Iti" });
                initSession.SaveChanges();
            }

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Empty(errors);
        }
    }
}
