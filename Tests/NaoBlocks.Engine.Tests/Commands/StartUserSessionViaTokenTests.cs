using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class StartUserSessionViaTokenTests : DatabaseHelper
    {
        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new StartUserSessionViaToken();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Token is required" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new StartUserSessionViaToken
            {
                Token = "12345"
            };
            using var store = InitialiseDatabase(new User { Id = "Tahi", Role = UserRole.Student, LoginToken = "12345" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateChecksForUser()
        {
            var command = new StartUserSessionViaToken
            {
                Token = "12345"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Unknown or invalid user" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ExecuteSavesNewSession()
        {
            var now = DateTime.Today;
            var command = new StartUserSessionViaToken
            {
                UserId = "Tahi",
                Role = UserRole.Student,
                WhenExecuted = now
            };
            using var store = InitialiseDatabase();

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                Assert.True(engine.DatabaseSession.StoreCalled, "Expected store call was missed");
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var record = verifySession.Query<Session>().FirstOrDefault();
            Assert.NotNull(record);
            Assert.Equal(UserRole.Student, record!.Role);
            Assert.Equal("Tahi", record!.UserId);
            Assert.Equal(now, record!.WhenAdded);
            Assert.Equal(now.AddDays(1), record!.WhenExpires);
        }

        [Fact]
        public async Task ExecuteSavesUpdatesNonExpiredSession()
        {
            var now = DateTime.Today;
            var command = new StartUserSessionViaToken
            {
                UserId = "Tahi",
                Role = UserRole.Student,
                WhenExecuted = now
            };
            using var store = InitialiseDatabase(new Session { UserId = "Tahi", WhenExpires = now.AddMinutes(1) });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
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
        public async Task ExecuteSavesNewSessionWithExpiredSession()
        {
            var now = DateTime.Today;
            var command = new StartUserSessionViaToken
            {
                UserId = "Tahi",
                Role = UserRole.Student,
                WhenExecuted = now
            };
            using var store = InitialiseDatabase(new Session { UserId = "Tahi", WhenExpires = now.AddMinutes(-1) });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                Assert.True(engine.DatabaseSession.StoreCalled, "Expected store call was missed");
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var record = verifySession.Query<Session>().FirstOrDefault(s => s.WhenAdded == now);
            Assert.NotNull(record);
            Assert.Equal(UserRole.Student, record!.Role);
            Assert.Equal("Tahi", record!.UserId);
            Assert.Equal(now, record!.WhenAdded);
            Assert.Equal(now.AddDays(1), record!.WhenExpires);
        }
    }
}
