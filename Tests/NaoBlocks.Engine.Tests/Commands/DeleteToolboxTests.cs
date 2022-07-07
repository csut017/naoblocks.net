using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class DeleteToolboxTests : DatabaseHelper
    {
        [Fact]
        public async Task ExecuteChecksInitialState()
        {
            var command = new DeleteToolbox
            {
                RobotTypeName = " Bob ",
            };
            var engine = new FakeEngine();
            var result = await engine.ExecuteAsync(command);
            Assert.False(result.WasSuccessful);
            Assert.Equal("Unexpected error: Command is not in a valid state. Need to call either ValidateAsync or RestoreAsync", result.Error);
        }

        [Fact]
        public async Task ExecuteDeletesToolbox()
        {
            var command = new DeleteToolbox
            {
                RobotTypeName = "Whero Iti",
                ToolboxName = "To Delete"
            };

            var robotType = new RobotType { Name = "Whero Iti" };
            robotType.Toolboxes.Add(new Toolbox { Name = "To Delete" });
            robotType.Toolboxes.Add(new Toolbox { Name = "To Leave" });
            using var store = InitialiseDatabase(robotType);

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var record = verifySession.Query<RobotType>().FirstOrDefault();
            Assert.Contains(record?.Toolboxes, t => t.Name == "To Leave");
            Assert.DoesNotContain(record?.Toolboxes, t => t.Name == "To Delete");
        }

        [Fact]
        public async Task ExecuteHandlesSingleToolbox()
        {
            var command = new DeleteToolbox
            {
                RobotTypeName = "Whero Iti",
                ToolboxName = "To Delete"
            };

            var robotType = new RobotType { Name = "Whero Iti" };
            robotType.Toolboxes.Add(new Toolbox { Name = "To Delete", IsDefault = true });
            using var store = InitialiseDatabase(robotType);

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var record = verifySession.Query<RobotType>().FirstOrDefault();
            Assert.Empty(record?.Toolboxes);
        }

        [Fact]
        public async Task ExecuteUpdatesDefault()
        {
            var command = new DeleteToolbox
            {
                RobotTypeName = "Whero Iti",
                ToolboxName = "To Delete"
            };

            var robotType = new RobotType { Name = "Whero Iti" };
            robotType.Toolboxes.Add(new Toolbox { Name = "To Delete", IsDefault = true });
            robotType.Toolboxes.Add(new Toolbox { Name = "To Leave" });
            using var store = InitialiseDatabase(robotType);

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var record = verifySession.Query<RobotType>().FirstOrDefault();
            Assert.Contains(record?.Toolboxes, t => t.IsDefault);
        }

        [Fact]
        public async Task RestoreFailsIfRobotIsMissing()
        {
            var command = new DeleteToolbox
            {
                RobotTypeName = "Whero Iti"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Equal(new[] { "Robot type Whero Iti does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreReloadsRobot()
        {
            var command = new DeleteToolbox
            {
                RobotTypeName = "Whero Iti"
            };
            using var store = InitialiseDatabase(new RobotType { Name = "Whero Iti" });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateChecksForExistingRobot()
        {
            var command = new DeleteToolbox
            {
                RobotTypeName = "Whero Iti",
                ToolboxName = "Testing"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Robot type Whero Iti does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new DeleteToolbox
            {
                RobotTypeName = "Whero Iti",
                ToolboxName = "Testing"
            };
            using var store = InitialiseDatabase(new RobotType { Name = "Whero Iti" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new DeleteToolbox();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(
                new[] { "Robot type name is required", "Toolbox name is required" },
                FakeEngine.GetErrors(errors));
        }
    }
}