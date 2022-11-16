using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class RenewSessionTests : DatabaseHelper
    {
        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new RenewSession();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "User name is required" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var now = DateTime.Today;
            var command = new RenewSession
            {
                UserName = "Bob",
                WhenExecuted = now
            };
            using var store = InitialiseDatabase(
                new User { Id = "Tahi", Role = UserRole.Student, Name = "Bob" },
                new Session {  UserId = "Tahi", WhenExpires = now.AddHours(1) });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateChecksSessionExists()
        {
            var now = DateTime.Today;
            var command = new RenewSession
            {
                UserName = "Bob",
                WhenExecuted = now
            };
            using var store = InitialiseDatabase(
                new User { Id = "Tahi", Role = UserRole.Student, Name = "Bob" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "User does not have a current session" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateChecksSessionIsValid()
        {
            var now = DateTime.Today;
            var command = new RenewSession
            {
                UserName = "Bob",
                WhenExecuted = now
            };
            using var store = InitialiseDatabase(
                new User { Id = "Tahi", Role = UserRole.Student, Name = "Bob" },
                new Session { UserId = "Tahi", WhenExpires = now.AddHours(-1) });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "User does not have a current session" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateChecksForUser()
        {
            var now = DateTime.Today;
            var command = new RenewSession
            {
                UserName = "Bob",
                WhenExecuted = now
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Unknown or invalid user" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ExecuteSavesUpdatesNonExpiredSession()
        {
            var now = DateTime.Today;
            var command = new RenewSession
            {
                UserName = "Bob",
                WhenExecuted = now
            };
            using var store = InitialiseDatabase(
                new User { Id = "Tahi", Role = UserRole.Student, Name = "Bob" },
                new Session { UserId = "Tahi", WhenExpires = now.AddMinutes(1) });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                Assert.False(engine.DatabaseSession.StoreCalled, "Unexpected store call made");
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var record = verifySession.Query<Session>().FirstOrDefault();
            Assert.NotNull(record);
            Assert.Equal(now.AddDays(1), record!.WhenExpires);
        }

        [Fact]
        public async Task RestoreFailsIfSessionIsMissing()
        {
            var now = DateTime.Today;
            var command = new RenewSession
            {
                UserName = "Bob",
                WhenExecuted = now
            };
            using var store = InitialiseDatabase(
                new User { Id = "Tahi", Role = UserRole.Student, Name = "Bob" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Equal(new[] { "Unable to retrieve session" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreFailsIfUserIsMissing()
        {
            var now = DateTime.Today;
            var command = new RenewSession
            {
                UserName = "Bob",
                WhenExecuted = now
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Equal(new[] { "Unable to retrieve user" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreReloadsSession()
        {
            var now = DateTime.Today;
            var command = new RenewSession
            {
                UserName = "Bob",
                WhenExecuted = now
            };
            using var store = InitialiseDatabase(
                new User { Id = "Tahi", Role = UserRole.Student, Name = "Bob" },
                new Session { UserId = "Tahi", WhenExpires = now.AddMinutes(1) });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Empty(errors);
        }
    }
}
