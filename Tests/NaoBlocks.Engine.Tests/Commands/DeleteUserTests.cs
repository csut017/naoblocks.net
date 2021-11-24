using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class DeleteUserTests : DatabaseHelper
    {
        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new DeleteUser();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "User name is required" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new DeleteUser
            {
                Role = UserRole.Student,
                Name = "Bob"
            };
            using var store = InitialiseDatabase(new User { Name = "Bob", Role = UserRole.Student });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateChecksForExistingUser()
        {
            var command = new DeleteUser
            {
                Role = UserRole.Student,
                Name = "Bob"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Student Bob does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ExecuteDeletesUser()
        {
            var command = new DeleteUser
            {
                Name = "Bob",
                Role = UserRole.Teacher
            };

            using var store = InitialiseDatabase(new User { Name = "Bob" });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            await engine.RestoreAsync(command);
            var result = await engine.ExecuteAsync(command);
            Assert.True(result.WasSuccessful);
            Assert.True(engine.DatabaseSession.DeletedCalled);
            Assert.NotNull(engine.DatabaseSession.GetLastModifiedEntity());
        }

        [Fact]
        public async Task ExecuteChecksInitialState()
        {
            var command = new DeleteUser
            {
                Name = " Bob ",
                Role = UserRole.Teacher
            };
            var engine = new FakeEngine();
            var result = await engine.ExecuteAsync(command);
            Assert.False(result.WasSuccessful);
            Assert.Equal("Unexpected error: Command is not in a valid state. Need to call either ValidateAsync or RestoreAsync", result.Error);
        }

        [Fact]
        public async Task RestoreFailsIfUserIsMissing()
        {
            var command = new DeleteUser
            {
                Role = UserRole.Student,
                Name = "Bob"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Equal(new[] { "Student Bob does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreReloadsUser()
        {
            var command = new DeleteUser
            {
                Role = UserRole.Student,
                Name = "Bob"
            };
            using var store = InitialiseDatabase(new User { Name = "Bob", Role = UserRole.Student });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Empty(errors);
        }
    }
}
