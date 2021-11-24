using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class AddBlockSetTests : DatabaseHelper
    {
        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new AddBlockSet();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Name is required for a block set", "Categories is required for a block set", "Robot Type is required for a block set" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new AddBlockSet
            {
                Name = "Tahi",
                RobotType = "Bobbot",
                Categories = "Rua"
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
            var command = new AddBlockSet
            {
                Name = "Tahi",
                RobotType = "Bobbot",
                Categories = "Rua"
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
            var command = new AddBlockSet
            {
                Name = "Tahi",
                RobotType = "Bobbot",
                Categories = "Rua"
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
            var command = new AddBlockSet
            {
                Name = "Tahi",
                RobotType = "Bobbot",
                Categories = "Rua"
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
            var command = new AddBlockSet
            {
                Name = "Tahi",
                RobotType = "Bobbot",
                Categories = "Rua"
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
            var blockset = robotType.BlockSets.FirstOrDefault();
            Assert.Equal("Tahi", blockset?.Name);
            Assert.Equal("Rua", blockset?.BlockCategories);
        }

        [Fact]
        public async Task ExecuteTrimsWhitespaceFromName()
        {
            var command = new AddBlockSet
            {
                Name = " Tahi ",
                RobotType = "Bobbot",
                Categories = "Rua"
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
            var blockset = robotType.BlockSets.FirstOrDefault();
            Assert.Equal("Tahi", blockset?.Name);
        }

        [Fact]
        public async Task ExecuteChecksInitialState()
        {
            var command = new AddBlockSet
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
