using System.Linq;
using System.Threading.Tasks;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class StoreSnapshotTests : DatabaseHelper
    {
        [Fact]
        public async Task ExecuteChecksInitialState()
        {
            var command = new StoreSnapshot
            {
                UserName = "Bob"
            };
            var engine = new FakeEngine();
            var result = await engine.ExecuteAsync(command);
            Assert.False(result.WasSuccessful);
            Assert.Equal("Unexpected error: Command is not in a valid state. Need to call either ValidateAsync or RestoreAsync", result.Error);
        }

        [Theory]
        [InlineData(null, "Unknown")]
        [InlineData("", "Unknown")]
        [InlineData(" ", "Unknown")]
        [InlineData("Application", "Application")]
        public async Task ExecuteHandlesSource(string? source, string expected)
        {
            var command = new StoreSnapshot
            {
                Source = source,
                UserName = "Bob",
                State = "go{}"
            };
            using var store = InitialiseDatabase(new User { Name = "Bob", Id = "Tahi" });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                await engine.CommitAsync();
                Assert.True(engine.DatabaseSession.StoreCalled, "An expected call to store was not made");
            }

            using var verifySession = store.OpenSession();
            var snapshot = verifySession.Query<Snapshot>().FirstOrDefault();
            Assert.NotNull(snapshot);
            Assert.Equal(expected, snapshot!.Source);
        }

        [Fact]
        public async Task ExecuteHandlesValues()
        {
            var command = new StoreSnapshot
            {
                UserName = "Bob",
                State = "go{}"
            };
            command.Values.Add(new NamedValue { Name = "Tahi", Value = "Rua" });
            using var store = InitialiseDatabase(new User { Name = "Bob", Id = "Tahi" });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                await engine.CommitAsync();
                Assert.True(engine.DatabaseSession.StoreCalled, "An expected call to store was not made");
            }

            using var verifySession = store.OpenSession();
            var snapshot = verifySession.Query<Snapshot>().FirstOrDefault();
            Assert.NotNull(snapshot);
            Assert.Equal(new[] { "Tahi" }, snapshot!.Values.Select(v => v.Name).ToArray());
            Assert.Equal(new[] { "Rua" }, snapshot!.Values.Select(v => v.Value).ToArray());
        }

        [Fact]
        public async Task ExecuteSavesSnapshot()
        {
            var command = new StoreSnapshot
            {
                UserName = "Bob",
                State = "go{}"
            };
            using var store = InitialiseDatabase(new User { Name = "Bob", Id = "Tahi" });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                await engine.CommitAsync();
                Assert.True(engine.DatabaseSession.StoreCalled, "An expected call to store was not made");
            }

            using var verifySession = store.OpenSession();
            var snapshot = verifySession.Query<Snapshot>().FirstOrDefault();
            Assert.NotNull(snapshot);
            Assert.Equal("Tahi", snapshot!.UserId);
            Assert.Equal("go{}", snapshot!.State);
        }

        [Fact]
        public async Task RestoreFailsIfUserIsMissing()
        {
            var command = new StoreSnapshot
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
        public async Task RestoreReloadsUser()
        {
            var command = new StoreSnapshot
            {
                UserName = "Bob"
            };
            using var store = InitialiseDatabase(new User { Name = "Bob", Role = UserRole.User });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateChecksForExistingUser()
        {
            var command = new StoreSnapshot
            {
                UserName = "Bob",
                State = "go{}"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "User Bob does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new StoreSnapshot
            {
                UserName = "Bob",
                State = "go{}"
            };
            using var store = InitialiseDatabase(new User { Name = "Bob", Role = UserRole.User });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new StoreSnapshot();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "State is required", "Username is required" }, FakeEngine.GetErrors(errors));
        }
    }
}