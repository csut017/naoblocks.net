using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class UpdateRobotTypeTests : DatabaseHelper
    {
        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new UpdateRobotType();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Current name is required" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new UpdateRobotType
            {
                CurrentName = "Bobbot"
            };
            using var store = InitialiseDatabase(new RobotType { Name = "Bobbot" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateChecksForExistingRobotType()
        {
            var command = new UpdateRobotType
            {
                CurrentName = "Bobbot"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Robot type Bobbot does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreFailsIfRobotTypeIsMissing()
        {
            var command = new UpdateRobotType
            {
                CurrentName = "Bobbot"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Equal(new[] { "Robot type Bobbot does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreReloadsRobotType()
        {
            var command = new UpdateRobotType
            {
                CurrentName = "Bobbot"
            };
            using var store = InitialiseDatabase(new RobotType { Name = "Bobbot" });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ExecuteSavesRobotType()
        {
            var command = new UpdateRobotType
            {
                Name = "BillBot",
                CurrentName = "Bobbot"
            };
            using var store = InitialiseDatabase(new RobotType { Name = "Bobbot" });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful);
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var robotType = verifySession.Query<RobotType>().First();
            Assert.Equal("BillBot", robotType.Name);
        }

        [Fact]
        public async Task ExecuteTrimsWhitespaceFromName()
        {
            var command = new UpdateRobotType
            {
                Name = " BillBot ",
                CurrentName = "Bobbot"
            };
            using var store = InitialiseDatabase(new RobotType { Name = "Bobbot" });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful);
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var robotType = verifySession.Query<RobotType>().First();
            Assert.Equal("BillBot", robotType.Name);
        }

        [Fact]
        public async Task ExecuteChecksInitialState()
        {
            var command = new UpdateRobotType
            {
                Name = "Bobbot"
            };
            var engine = new FakeEngine();
            var result = await engine.ExecuteAsync(command);
            Assert.False(result.WasSuccessful);
            Assert.Equal("Unexpected error: Command is not in a valid state. Need to call either ValidateAsync or RestoreAsync", result.Error);
        }
    }
}
