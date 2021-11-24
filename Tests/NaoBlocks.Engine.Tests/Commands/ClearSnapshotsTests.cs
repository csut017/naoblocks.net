﻿using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class ClearSnapshotsTests : DatabaseHelper
    {
        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new ClearSnapshots();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Username is required" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new ClearSnapshots
            {
                UserName = "Bob"
            };
            using var store = InitialiseDatabase(new User { Name = "Bob", Role = UserRole.User });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateChecksForExistingUser()
        {
            var command = new ClearSnapshots
            {
                UserName = "Bob"
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
            var command = new ClearSnapshots
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
            var command = new ClearSnapshots
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
        public async Task ExecuteClearsSnapshot()
        {
            var command = new ClearSnapshots
            {
                UserName = "Bob"
            };
            using var store = InitialiseDatabase(
                new User { Name = "Bob", Id = "Tahi" },
                new Snapshot { UserId = "Tahi" });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                await engine.CommitAsync();
                Assert.True(engine.DatabaseSession.DeletedCalled, "An expected call to store was not made");
            }

            using var verifySession = store.OpenSession();
            var snapshots = verifySession.Query<Snapshot>().ToArray();
            Assert.Empty(snapshots);
        }

        [Fact]
        public async Task ExecuteChecksInitialState()
        {
            var command = new ClearSnapshots
            {
                UserName = "Bob"
            };
            var engine = new FakeEngine();
            var result = await engine.ExecuteAsync(command);
            Assert.False(result.WasSuccessful);
            Assert.Equal("Unexpected error: Command is not in a valid state. Need to call either ValidateAsync or RestoreAsync", result.Error);
        }
    }
}
