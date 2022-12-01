using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class UpdateCustomValuesForRobotTests
         : DatabaseHelper
    {
        [Fact]
        public async Task ExecuteAddsNewValues()
        {
            var command = new UpdateCustomValuesForRobot
            {
                MachineName = "Mihīni",
                Values = new[]
                {
                    new NamedValue {Name = "One", Value = "Tahi"},
                    new NamedValue {Name = "Two", Value = "Rua"},
                }
            };
            var databaseRobot = new Robot { MachineName = "Mihīni" };
            databaseRobot.CustomValues.Add(new NamedValue { Name = "One", Value = "Tahi" });
            using var store = InitialiseDatabase(
                databaseRobot);

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var loadedRobot = verifySession.Query<Robot>().FirstOrDefault();
            Assert.Equal(
                new[] { "One=Tahi", "Two=Rua" },
                loadedRobot?.CustomValues.Select(nv => $"{nv.Name}={nv.Value}").ToArray());
        }

        [Fact]
        public async Task ExecuteChecksInitialState()
        {
            var command = new UpdateCustomValuesForRobot
            {
                MachineName = "Mihīni"
            };
            var engine = new FakeEngine();
            var result = await engine.ExecuteAsync(command);
            Assert.False(result.WasSuccessful);
            Assert.Equal("Unexpected error: Command is not in a valid state. Need to call either ValidateAsync or RestoreAsync", result.Error);
        }

        [Fact]
        public async Task ExecuteDeletesMissingValues()
        {
            var command = new UpdateCustomValuesForRobot
            {
                MachineName = "Mihīni",
                Values = new[]
                {
                    new NamedValue {Name = "One", Value = "Tahi"},
                }
            };
            var databaseRobot = new Robot { MachineName = "Mihīni" };
            databaseRobot.CustomValues.Add(new NamedValue { Name = "One", Value = "Tahi" });
            databaseRobot.CustomValues.Add(new NamedValue { Name = "Three", Value = "Toru" });
            using var store = InitialiseDatabase(
                databaseRobot);

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var loadedRobot = verifySession.Query<Robot>().FirstOrDefault();
            Assert.Equal(
                new[] { "One=Tahi" },
                loadedRobot?.CustomValues.Select(nv => $"{nv.Name}={nv.Value}").ToArray());
        }

        [Fact]
        public async Task ExecuteIgnoresNullValues()
        {
            var command = new UpdateCustomValuesForRobot
            {
                MachineName = "Mihīni",
                Values = new[]
                {
                    new NamedValue {Name = "One", Value = "Tahi"},
                    null,
                }
            };
            var databaseRobot = new Robot { MachineName = "Mihīni" };
            databaseRobot.CustomValues.Add(new NamedValue { Name = "One", Value = "Tahi" });
            using var store = InitialiseDatabase(
                databaseRobot);

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var loadedRobot = verifySession.Query<Robot>().FirstOrDefault();
            Assert.Equal(
                new[] { "One=Tahi" },
                loadedRobot?.CustomValues.Select(nv => $"{nv.Name}={nv.Value}").ToArray());
        }

        [Fact]
        public async Task ExecuteSavesSettings()
        {
            var command = new UpdateCustomValuesForRobot
            {
                MachineName = "Mihīni",
                Values = new[]
                {
                    new NamedValue {Name = "One", Value = "Tahi"},
                    new NamedValue {Name = "Two", Value = "Rua"},
                    new NamedValue {Name = "Four", Value = "Whā"},
                }
            };
            var databaseRobot = new Robot { MachineName = "Mihīni" };
            databaseRobot.CustomValues.Add(new NamedValue { Name = "One", Value = "Tahi" });
            databaseRobot.CustomValues.Add(new NamedValue { Name = "Three", Value = "Toru" });
            databaseRobot.CustomValues.Add(new NamedValue { Name = "Four", Value = "Four" });
            using var store = InitialiseDatabase(
                databaseRobot);

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var loadedRobot = verifySession.Query<Robot>().FirstOrDefault();
            Assert.Equal(
                new[] { "One=Tahi", "Two=Rua", "Four=Whā" },
                loadedRobot?.CustomValues.Select(nv => $"{nv.Name}={nv.Value}").ToArray());
        }

        [Fact]
        public async Task ExecuteUpdatesExistingValues()
        {
            var command = new UpdateCustomValuesForRobot
            {
                MachineName = "Mihīni",
                Values = new[]
                {
                    new NamedValue {Name = "One", Value = "Tahi"},
                }
            };
            var databaseRobot = new Robot { MachineName = "Mihīni" };
            databaseRobot.CustomValues.Add(new NamedValue { Name = "One", Value = "One" });
            using var store = InitialiseDatabase(
                databaseRobot);

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var loadedRobot = verifySession.Query<Robot>().FirstOrDefault();
            Assert.Equal(
                new[] { "One=Tahi" },
                loadedRobot?.CustomValues.Select(nv => $"{nv.Name}={nv.Value}").ToArray());
        }

        [Fact]
        public async Task RestoreFailsIfRobotIsMissing()
        {
            var command = new UpdateCustomValuesForRobot
            {
                MachineName = "Mihīni"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Equal(new[] { "Unknown robot 'Mihīni'" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreReloadsEntities()
        {
            var command = new UpdateCustomValuesForRobot
            {
                MachineName = "Mihīni"
            };
            using var store = InitialiseDatabase(
                new Robot { MachineName = "Mihīni" });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateChecksForDuplicateNameInData()
        {
            var command = new UpdateCustomValuesForRobot
            {
                MachineName = "Mihīni",
                Values = new[]
                {
                    NamedValue.New("Exists", string.Empty),
                    NamedValue.New("Exists", string.Empty),
                }
            };
            var robotType = new Robot { MachineName = "Mihīni" };
            using var store = InitialiseDatabase(
                robotType);
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Value 'Exists' is a duplicated name" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateChecksForExistingRobot()
        {
            var command = new UpdateCustomValuesForRobot
            {
                MachineName = "Mihīni"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Unknown robot 'Mihīni'" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateIgnoresNullValues()
        {
            var command = new UpdateCustomValuesForRobot
            {
                MachineName = "Mihīni",
                Values = new[]
                {
                    NamedValue.New("One", "Tahi"),
                    null,
                }
            };
            using var store = InitialiseDatabase(
                new Robot { MachineName = "Mihīni" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new UpdateCustomValuesForRobot
            {
                MachineName = "Mihīni",
                Values = new[]
                {
                    NamedValue.New("One", "Tahi"),
                }
            };
            using var store = InitialiseDatabase(
                new Robot { MachineName = "Mihīni" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new UpdateCustomValuesForRobot();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Machine name is required" }, FakeEngine.GetErrors(errors));
        }
    }
}