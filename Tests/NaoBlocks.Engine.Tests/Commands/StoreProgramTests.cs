using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class StoreProgramTests : DatabaseHelper
    {
        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new StoreProgram();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Code is required for storing a program", "User name is required" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new StoreProgram
            {
                UserName = "Bob",
                Code = "go{}"
            };
            using var store = InitialiseDatabase(new User { Name = "Bob", Role = UserRole.User });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Theory]
        [InlineData(false, null)]
        [InlineData(false, "Wha")]
        [InlineData(true, null, "Name is required for storing a program")]
        [InlineData(true, "Wha")]
        public async Task ValidateChecksForName(bool requireName, string? name, params string[] expectedErrors)
        {
            var command = new StoreProgram
            {
                UserName = "Bob",
                Code = "go{}",
                RequireName = requireName,
                Name = name
            };
            using var store = InitialiseDatabase(new User { Name = "Bob", Role = UserRole.User });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(expectedErrors, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateChecksForExistingUser()
        {
            var command = new StoreProgram
            {
                UserName = "Bob",
                Code = "go{}"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "User Bob does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreFailsIfUserIsMissing()
        {
            var command = new StoreProgram
            {
                UserName = "Bob",
                Code = "go{}"
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
            var command = new StoreProgram
            {
                UserName = "Bob",
                Code = "go{}"
            };
            using var store = InitialiseDatabase(new User { Name = "Bob", Role = UserRole.User });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ExecuteSavesProgram()
        {
            var command = new StoreProgram
            {
                UserName = "Bob",
                Code = "go{}"
            };
            using var store = InitialiseDatabase(new User { Name = "Bob", Id = "Tahi" });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful);
                await engine.CommitAsync();
                Assert.True(engine.DatabaseSession.StoreCalled, "An expected call to store was not made");
            }

            using var verifySession = store.OpenSession();
            var program = verifySession.Query<CodeProgram>().FirstOrDefault();
            Assert.NotNull(program);
            Assert.Equal("Tahi", program!.UserId);
            Assert.Equal("go{}", program!.Code);
        }

        [Fact]
        public async Task ExecuteUpdatesExistingProgram()
        {
            var command = new StoreProgram
            {
                UserName = "Bob",
                Name = "Wha",
                Code = "rest()"
            };
            using var store = InitialiseDatabase(
                new User { Name = "Bob", Id = "Tahi" },
                new CodeProgram { Code = "go{}", Name = "Wha", UserId = "Tahi" });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                await engine.CommitAsync();
                Assert.False(engine.DatabaseSession.StoreCalled, "An unexpected call to store was made");
            }

            using var verifySession = store.OpenSession();
            var program = verifySession.Query<CodeProgram>().FirstOrDefault();
            Assert.NotNull(program);
            Assert.Equal("rest()", program!.Code);
        }

        [Fact]
        public async Task ExecuteTrimsWhitespaceFromName()
        {
            var command = new StoreProgram
            {
                UserName = "Bob",
                Name = " Rua ",
                Code = "go{}"
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
            var program = verifySession.Query<CodeProgram>().FirstOrDefault();
            Assert.NotNull(program);
            Assert.Equal("Rua", program!.Name);
        }

        [Fact]
        public async Task ExecuteChecksInitialState()
        {
            var command = new StoreProgram
            {
                UserName = "Bob",
                Code = "go{}"
            };
            var engine = new FakeEngine();
            var result = await engine.ExecuteAsync(command);
            Assert.False(result.WasSuccessful);
            Assert.Equal("Unexpected error: Command is not in a valid state. Need to call either ValidateAsync or RestoreAsync", result.Error);
        }
    }
}
