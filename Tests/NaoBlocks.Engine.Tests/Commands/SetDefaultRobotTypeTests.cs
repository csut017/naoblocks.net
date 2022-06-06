﻿using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class SetDefaultRobotTypeTests : DatabaseHelper
    {
        [Fact]
        public async Task ExecuteChecksInitialState()
        {
            var command = new SetDefaultRobotType
            {
                Name = "Bobbot"
            };
            var engine = new FakeEngine();
            var result = await engine.ExecuteAsync(command);
            Assert.False(result.WasSuccessful);
            Assert.Equal("Unexpected error: Command is not in a valid state. Need to call either ValidateAsync or RestoreAsync", result.Error);
        }

        [Fact]
        public async Task ExecuteClearsPreviousDefault()
        {
            var command = new SetDefaultRobotType
            {
                Name = "Bobbot"
            };
            using var store = InitialiseDatabase(
                new RobotType { Name = "Bobbot" },
                new RobotType { Name = "Billbot", IsDefault = true });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful);
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var robotType = verifySession.Query<RobotType>().First(rt => rt.Name == "Bobbot");
            Assert.True(robotType.IsDefault);
            robotType = verifySession.Query<RobotType>().First(rt => rt.Name == "Billbot");
            Assert.False(robotType.IsDefault);
        }

        [Fact]
        public async Task ExecuteIgnoresIfAlreadyDefault()
        {
            var command = new SetDefaultRobotType
            {
                Name = "Bobbot"
            };
            using var store = InitialiseDatabase(new RobotType { Name = "Bobbot", IsDefault = true });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful);
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var robotType = verifySession.Query<RobotType>().First();
            Assert.True(robotType.IsDefault);
        }

        [Fact]
        public async Task ExecuteSetsDefaultRobotType()
        {
            var command = new SetDefaultRobotType
            {
                Name = "Bobbot"
            };
            using var store = InitialiseDatabase(new RobotType { Name = "Bobbot" });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful);
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var robotType = verifySession.Query<RobotType>().First();
            Assert.True(robotType.IsDefault);
        }

        [Fact]
        public async Task ExecuteUsesCachedRobotType()
        {
            var command = new SetDefaultRobotType
            {
                Name = "Bobbot",
                IgnoreMissingRobotType = true
            };
            using var store = InitialiseDatabase(new RobotType());

            var cachedRobotType = new RobotType { Name = "Bobbot" };
            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                engine.DatabaseSession.CacheItem("Bobbot", cachedRobotType);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful);
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            Assert.True(cachedRobotType.IsDefault);
        }

        [Fact]
        public async Task RestoreFailsIfRobotTypeIsMissing()
        {
            var command = new SetDefaultRobotType
            {
                Name = "Bobbot"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Equal(new[] { "Robot type Bobbot does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreReloadsRobotType()
        {
            var command = new SetDefaultRobotType
            {
                Name = "Bobbot"
            };
            using var store = InitialiseDatabase(new RobotType { Name = "Bobbot" });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateChecksForExistingRobotType()
        {
            var command = new SetDefaultRobotType
            {
                Name = "Bobbot"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Robot type Bobbot does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new SetDefaultRobotType
            {
                Name = "Bobbot"
            };
            using var store = InitialiseDatabase(new RobotType { Name = "Bobbot" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new SetDefaultRobotType();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Robot type name is required" }, FakeEngine.GetErrors(errors));
        }
    }
}