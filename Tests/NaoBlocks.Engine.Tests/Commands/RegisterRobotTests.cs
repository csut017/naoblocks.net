using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class RegisterRobotTests : DatabaseHelper
    {
        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new RegisterRobot();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Machine name is required for a robot" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateChecksForExistingRobot()
        {
            var command = new RegisterRobot
            {
                MachineName = "Whero Iti"
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
        public async Task ValidatePassesChecks()
        {
            var command = new RegisterRobot
            {
                MachineName = "Whero Iti"
            };
            using var store = InitialiseDatabase();

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ExecuteAddsRobot()
        {
            var command = new RegisterRobot
            {
                MachineName = "Whero Iti"
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
            Assert.Equal("Whero Iti", robot.FriendlyName);
            Assert.False(robot.IsInitialised);
        }
    }
}
