using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class UpdateCustomValuesForRobotTypeTests
         : DatabaseHelper
    {
        [Fact]
        public async Task ExecuteAddsNewValues()
        {
            var command = new UpdateCustomValuesForRobotType
            {
                Name = "Mihīni",
                Values = new[]
                {
                    new NamedValue {Name = "One", Value = "Tahi"},
                    new NamedValue {Name = "Two", Value = "Rua"},
                }
            };
            var databaseRobotType = new RobotType { Name = "Mihīni" };
            databaseRobotType.CustomValues.Add(new NamedValue { Name = "One", Value = "Tahi" });
            using var store = InitialiseDatabase(
                databaseRobotType);

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var loadedRobotType = verifySession.Query<RobotType>().FirstOrDefault();
            Assert.Equal(
                new[] { "One=Tahi", "Two=Rua" },
                loadedRobotType?.CustomValues.Select(nv => $"{nv.Name}={nv.Value}").ToArray());
        }

        [Fact]
        public async Task ExecuteChecksInitialState()
        {
            var command = new UpdateCustomValuesForRobotType
            {
                Name = "Mihīni"
            };
            var engine = new FakeEngine();
            var result = await engine.ExecuteAsync(command);
            Assert.False(result.WasSuccessful);
            Assert.Equal("Unexpected error: Command is not in a valid state. Need to call either ValidateAsync or RestoreAsync", result.Error);
        }

        [Fact]
        public async Task ExecuteDeletesMissingValues()
        {
            var command = new UpdateCustomValuesForRobotType
            {
                Name = "Mihīni",
                Values = new[]
                {
                    new NamedValue {Name = "One", Value = "Tahi"},
                }
            };
            var databaseRobotType = new RobotType { Name = "Mihīni" };
            databaseRobotType.CustomValues.Add(new NamedValue { Name = "One", Value = "Tahi" });
            databaseRobotType.CustomValues.Add(new NamedValue { Name = "Three", Value = "Toru" });
            using var store = InitialiseDatabase(
                databaseRobotType);

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var loadedRobotType = verifySession.Query<RobotType>().FirstOrDefault();
            Assert.Equal(
                new[] { "One=Tahi" },
                loadedRobotType?.CustomValues.Select(nv => $"{nv.Name}={nv.Value}").ToArray());
        }

        [Fact]
        public async Task ExecuteSavesSettings()
        {
            var command = new UpdateCustomValuesForRobotType
            {
                Name = "Mihīni",
                Values = new[]
                {
                    new NamedValue {Name = "One", Value = "Tahi"},
                    new NamedValue {Name = "Two", Value = "Rua"},
                    new NamedValue {Name = "Four", Value = "Whā"},
                }
            };
            var databaseRobotType = new RobotType { Name = "Mihīni" };
            databaseRobotType.CustomValues.Add(new NamedValue { Name = "One", Value = "Tahi" });
            databaseRobotType.CustomValues.Add(new NamedValue { Name = "Three", Value = "Toru" });
            databaseRobotType.CustomValues.Add(new NamedValue { Name = "Four", Value = "Four" });
            using var store = InitialiseDatabase(
                databaseRobotType);

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var loadedRobotType = verifySession.Query<RobotType>().FirstOrDefault();
            Assert.Equal(
                new[] { "One=Tahi", "Two=Rua", "Four=Whā" },
                loadedRobotType?.CustomValues.Select(nv => $"{nv.Name}={nv.Value}").ToArray());
        }

        [Fact]
        public async Task ExecuteUpdatesExistingValues()
        {
            var command = new UpdateCustomValuesForRobotType
            {
                Name = "Mihīni",
                Values = new[]
                {
                    new NamedValue {Name = "One", Value = "Tahi"},
                }
            };
            var databaseRobotType = new RobotType { Name = "Mihīni" };
            databaseRobotType.CustomValues.Add(new NamedValue { Name = "One", Value = "One" });
            using var store = InitialiseDatabase(
                databaseRobotType);

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var loadedRobotType = verifySession.Query<RobotType>().FirstOrDefault();
            Assert.Equal(
                new[] { "One=Tahi" },
                loadedRobotType?.CustomValues.Select(nv => $"{nv.Name}={nv.Value}").ToArray());
        }

        [Fact]
        public async Task RestoreFailsIfRobotTypeIsMissing()
        {
            var command = new UpdateCustomValuesForRobotType
            {
                Name = "Mihīni"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Equal(new[] { "Unknown robot type 'Mihīni'" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreReloadsEntities()
        {
            var command = new UpdateCustomValuesForRobotType
            {
                Name = "Mihīni"
            };
            using var store = InitialiseDatabase(
                new RobotType { Name = "Mihīni" });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateChecksForExistingRobotType()
        {
            var command = new UpdateCustomValuesForRobotType
            {
                Name = "Mihīni"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Unknown robot type 'Mihīni'" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new UpdateCustomValuesForRobotType
            {
                Name = "Mihīni"
            };
            using var store = InitialiseDatabase(
                new RobotType { Name = "Mihīni" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new UpdateCustomValuesForRobotType();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Robot type name is required" }, FakeEngine.GetErrors(errors));
        }
    }
}