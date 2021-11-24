using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class AddRobotTests : DatabaseHelper
    {
        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new AddRobot();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Machine name is required for a robot", "Type is required for a robot" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateChecksForExistingRobot()
        {
            var command = new AddRobot
            {
                MachineName = "Whero Iti",
                Type = "Bobbot"
            };
            using var store = InitialiseDatabase(
                new Robot { MachineName = "Whero Iti" },
                new RobotType { Name = "Bobbot" });
                
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Robot with name Whero Iti already exists" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateChecksForRobotType()
        {
            var command = new AddRobot
            {
                MachineName = "Whero Iti",
                Type = "Bobbot"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Unknown robot type Bobbot" }, FakeEngine.GetErrors(errors));
        }

        [Theory]
        [InlineData(null, "mihīni", "mihīni")]
        [InlineData("", "mihīni", "mihīni")]
        [InlineData(" ", "mihīni", "mihīni")]
        [InlineData("Whero iti", "mihīni", "Whero iti")]
        public async Task ValidateHandlesFriendlyName(string? friendly, string machine, string expected)
        {
            var command = new AddRobot
            {
                MachineName = machine,
                FriendlyName = friendly
            };
            var engine = new FakeEngine();
            await engine.ValidateAsync(command);
            Assert.Equal(expected, command.FriendlyName);
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new AddRobot
            {
                MachineName = "Whero Iti",
                Type = "Bobbot"
            };
            using var store = InitialiseDatabase(
                new RobotType { Name = "Bobbot" });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateHashesPassword()
        {
            var command = new AddRobot
            {
                Password = "1234"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            await engine.ValidateAsync(command);
            Assert.Null(command.Password);
            Assert.NotNull(command.HashedPassword);
            Assert.True(command.HashedPassword.Verify("1234"));
        }

        [Fact]
        public async Task ExecuteAddsRobot()
        {
            var command = new AddRobot
            {
                MachineName = "Whero Iti",
                FriendlyName = "Whero",
                Type = "Bobbot",
                HashedPassword = Password.New("1234")
            };
            using var store = InitialiseDatabase(
                new RobotType { Name = "Bobbot", Id = "Rua" });
                

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            await engine.RestoreAsync(command);
            var result = await engine.ExecuteAsync(command);

            Assert.True(result.WasSuccessful, "Command was not successful");
            Assert.True(engine.DatabaseSession.StoreCalled);
            var robot = Assert.IsType<Robot>(engine.DatabaseSession.GetLastModifiedEntity());
            Assert.Equal("Whero Iti", robot.MachineName);
            Assert.Equal("Whero", robot.FriendlyName);
            Assert.Equal("Rua", robot.RobotTypeId);
            Assert.True(robot.Password.Verify("1234"));
        }

        [Fact]
        public async Task RestoreFailsIfRobotTypeIsMissing()
        {
            var command = new AddRobot
            {
                MachineName = "Whero Iti",
                FriendlyName = "Whero",
                Type = "Bobbot",
                HashedPassword = Password.New("1234")
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Equal(new[] { "Unknown robot type Bobbot" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreReloadsRobotType()
        {
            var command = new AddRobot
            {
                MachineName = "Whero Iti",
                FriendlyName = "Whero",
                Type = "Bobbot",
                HashedPassword = Password.New("1234")
            };
            using var store = InitialiseDatabase(
                new RobotType { Name = "Bobbot" });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Empty(errors);
        }
    }
}
