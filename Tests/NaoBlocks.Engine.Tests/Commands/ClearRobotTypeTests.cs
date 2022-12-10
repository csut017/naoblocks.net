using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class ClearRobotTypeTests : DatabaseHelper
    {
        [Fact]
        public async Task ExecuteChecksInitialState()
        {
            var command = new ClearRobotType
            {
                Name = "Bobbot"
            };
            var engine = new FakeEngine();
            var result = await engine.ExecuteAsync(command);
            Assert.False(result.WasSuccessful);
            Assert.Equal("Unexpected error: Command is not in a valid state. Need to call either ValidateAsync or RestoreAsync", result.Error);
        }

        [Fact]
        public async Task ExecuteWipesAll()
        {
            var command = new ClearRobotType
            {
                Name = "Bobbot",
                IncludeCustomValues = true,
                IncludeLoggingTemplates = true,
                IncludeToolboxes = true,
            };
            using var store = InitialiseDatabase(GenerateDefaultRobotType());

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
            Assert.Empty(robotType.CustomValues);
            Assert.Empty(robotType.LoggingTemplates);
            Assert.Empty(robotType.Toolboxes);
        }

        [Fact]
        public async Task ExecuteWipesCustomValues()
        {
            var command = new ClearRobotType
            {
                Name = "Bobbot",
                IncludeCustomValues = true,
            };
            using var store = InitialiseDatabase(GenerateDefaultRobotType());

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
            Assert.Empty(robotType.CustomValues);
            Assert.NotEmpty(robotType.LoggingTemplates);
            Assert.NotEmpty(robotType.Toolboxes);
        }

        [Fact]
        public async Task ExecuteWipesLoggingTemplates()
        {
            var command = new ClearRobotType
            {
                Name = "Bobbot",
                IncludeLoggingTemplates = true,
            };
            using var store = InitialiseDatabase(GenerateDefaultRobotType());

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
            Assert.NotEmpty(robotType.CustomValues);
            Assert.Empty(robotType.LoggingTemplates);
            Assert.NotEmpty(robotType.Toolboxes);
        }

        [Fact]
        public async Task ExecuteWipesToolboxes()
        {
            var command = new ClearRobotType
            {
                Name = "Bobbot",
                IncludeToolboxes = true,
            };
            using var store = InitialiseDatabase(GenerateDefaultRobotType());

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
            Assert.NotEmpty(robotType.CustomValues);
            Assert.NotEmpty(robotType.LoggingTemplates);
            Assert.Empty(robotType.Toolboxes);
        }

        [Fact]
        public async Task RestoreFailsIfRobotTypeIsMissing()
        {
            var command = new ClearRobotType
            {
                Name = "Bobbot"
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
            var command = new ClearRobotType
            {
                Name = "Bobbot"
            };
            using var store = InitialiseDatabase(new RobotType { Name = "Bobbot" });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateChecksForExistingRobotType()
        {
            var command = new ClearRobotType
            {
                Name = "Bobbot"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Robot type Bobbot does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new ClearRobotType
            {
                Name = "Bobbot"
            };
            using var store = InitialiseDatabase(new RobotType { Name = "Bobbot" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new ClearRobotType();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Name is required" }, FakeEngine.GetErrors(errors));
        }

        private static RobotType GenerateDefaultRobotType()
        {
            var robotType = new RobotType { Name = "Bobbot" };
            robotType.CustomValues.Add(new NamedValue());
            robotType.LoggingTemplates.Add(new LoggingTemplate());
            robotType.Toolboxes.Add(new Toolbox());
            return robotType;
        }
    }
}