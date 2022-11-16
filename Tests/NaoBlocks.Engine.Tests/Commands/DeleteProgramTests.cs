using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class DeleteProgramTests : DatabaseHelper
    {
        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new DeleteProgram();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] {
                "Program number is required"
            }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new DeleteProgram
            {
                ProgramNumber = 34,
                UserName = "Bob"
            };
            using var store = InitialiseDatabase(new User { Name = "Bob" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateChecksForExistingUser()
        {
            var command = new DeleteProgram
            {
                ProgramNumber = 34,
                UserName = "Bob"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "User Bob does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ExecuteDeletesProgram()
        {
            var command = new DeleteProgram
            {
                ProgramNumber = 34,
                UserName = "Bob"
            };

            using var store = InitialiseDatabase(
                new User { Name = "Bob", Id = "users/1" },
                new CodeProgram { UserId = "users/1", Number = 34 });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            await engine.RestoreAsync(command);
            var result = await engine.ExecuteAsync(command);
            Assert.True(result.WasSuccessful);
            Assert.True(engine.DatabaseSession.DeletedCalled);
            Assert.NotNull(engine.DatabaseSession.GetLastModifiedEntity());
        }

        [Fact]
        public async Task ExecuteHandlesMissingProgram()
        {
            var command = new DeleteProgram
            {
                ProgramNumber = 34,
                UserName = "Bob"
            };

            using var store = InitialiseDatabase(
                new User { Name = "Bob", Id = "users/1" });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            await engine.RestoreAsync(command);
            var result = await engine.ExecuteAsync(command);
            Assert.True(result.WasSuccessful);
            Assert.False(engine.DatabaseSession.DeletedCalled);
        }

        [Fact]
        public async Task ExecuteChecksInitialState()
        {
            var command = new DeleteProgram
            {
                ProgramNumber = 34,
                UserName = "Bob"
            };
            var engine = new FakeEngine();
            var result = await engine.ExecuteAsync(command);
            Assert.False(result.WasSuccessful);
            Assert.Equal("Unexpected error: Command is not in a valid state. Need to call either ValidateAsync or RestoreAsync", result.Error);
        }

        [Fact]
        public async Task RestoreFailsIfUserIsMissing()
        {
            var command = new DeleteProgram
            {
                ProgramNumber = 34,
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
            var command = new DeleteProgram
            {
                ProgramNumber = 34,
                UserName = "Bob"
            };
            using var store = InitialiseDatabase(new User { Name = "Bob" });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Empty(errors);
        }
    }
}
