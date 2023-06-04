using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class GenerateLoginTokenTests : DatabaseHelper
    {
        [Fact]
        public async Task ExecuteChecksInitialState()
        {
            var command = new GenerateLoginToken
            {
                Name = " Bob "
            };
            var engine = new FakeEngine();
            var result = await engine.ExecuteAsync(command);
            Assert.False(result.WasSuccessful);
            Assert.Equal("Unexpected error: Command is not in a valid state. Need to call either ValidateAsync or RestoreAsync", result.Error);
        }

        [Fact]
        public async Task ExecuteSetsSessionToken()
        {
            var command = new GenerateLoginToken
            {
                Name = "Bob",
                SecurityToken = "Testing"
            };
            using var store = InitialiseDatabase(new User { Name = "Bob" });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful);
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var user = verifySession.Query<User>().First();
            Assert.False(string.IsNullOrEmpty(user.LoginToken?.Hash));
        }

        [Fact]
        public async Task RestoreFailsIfUserIsMissing()
        {
            var command = new GenerateLoginToken
            {
                Name = "Bob"
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
            var command = new GenerateLoginToken
            {
                Name = "Bob"
            };
            using var store = InitialiseDatabase(new User { Name = "Bob" });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateChecksForExistingUser()
        {
            var command = new GenerateLoginToken
            {
                Name = "Bob"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "User Bob does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateGeneratesToken()
        {
            var command = new GenerateLoginToken
            {
                Name = "Bob"
            };
            using var store = InitialiseDatabase(new User { Name = "Bob" });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            await engine.ValidateAsync(command);
            Assert.False(string.IsNullOrEmpty(command.SecurityToken));
        }

        [Fact]
        public async Task ValidateOverridesToken()
        {
            var command = new GenerateLoginToken
            {
                Name = "Bob",
                SecurityToken = "OldToken",
            };
            using var store = InitialiseDatabase(new User { Name = "Bob" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            await engine.ValidateAsync(command);
            Assert.NotEqual("OldToken", command.SecurityToken);
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new GenerateLoginToken
            {
                Name = "Bob"
            };
            using var store = InitialiseDatabase(new User { Name = "Bob" });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new GenerateLoginToken();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "User name is required" }, FakeEngine.GetErrors(errors));
        }
    }
}