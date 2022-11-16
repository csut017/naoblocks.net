using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class StoreSettingsTests : DatabaseHelper
    {
        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new StoreSettings();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Settings are required", "User name is required" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new StoreSettings
            {
                UserName = "Bob",
                Settings = new UserSettings
                {
                    RobotType = "Bobbot"
                }
            };
            using var store = InitialiseDatabase(
                new User { Name = "Bob" }, 
                new RobotType { Name = "Bobbot" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateChecksForExistingRobot()
        {
            var command = new StoreSettings
            {
                UserName = "Bob",
                Settings = new UserSettings
                {
                    RobotType = "Bobbot"
                }
            };
            using var store = InitialiseDatabase(new User { Name = "Bob" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Unknown robot type Bobbot" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateChecksForExistingUser()
        {
            var command = new StoreSettings
            {
                UserName = "Bob",
                Settings = new UserSettings
                {
                    RobotType = "Bobbot"
                }
            };
            using var store = InitialiseDatabase(new RobotType { Name = "Bobbot" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "User Bob does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreFailsIfUserIsMissing()
        {
            var command = new StoreSettings
            {
                UserName = "Bob"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Equal(new[] { "User Bob does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreFailsIfRobotTypeIsMissing()
        {
            var command = new StoreSettings
            {
                UserName = "Bob",
                Settings = new UserSettings { RobotType = "Bobbot" }
            };
            using var store = InitialiseDatabase(new User { Name = "Bob", Role = UserRole.User });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Equal(new[] { "Unknown robot type Bobbot" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreReloadsEntities()
        {
            var command = new StoreSettings
            {
                UserName = "Bob",
                Settings = new UserSettings { RobotType = "Bobbot" }
            };
            using var store = InitialiseDatabase(
                new User { Name = "Bob", Role = UserRole.User },
                new RobotType { Name = "Bobbot" });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ExecuteSavesSettings()
        {
            var command = new StoreSettings
            {
                UserName = "Bob",
                Settings = new UserSettings { RobotType = "Bobbot" }
            };
            using var store = InitialiseDatabase(
                new User { Name = "Bob", Role = UserRole.User },
                new RobotType { Name = "Bobbot", Id = "Tahi" });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var user = verifySession.Query<User>().FirstOrDefault();
            Assert.NotNull(user?.Settings);
            Assert.Equal("Tahi", user!.Settings!.RobotTypeId);
        }

        [Fact]
        public async Task ExecuteChecksInitialState()
        {
            var command = new StoreSettings
            {
                UserName = "Bob"
            };
            var engine = new FakeEngine();
            var result = await engine.ExecuteAsync(command);
            Assert.False(result.WasSuccessful);
            Assert.Equal("Unexpected error: Command is not in a valid state. Need to call either ValidateAsync or RestoreAsync", result.Error);
        }
    }
}
