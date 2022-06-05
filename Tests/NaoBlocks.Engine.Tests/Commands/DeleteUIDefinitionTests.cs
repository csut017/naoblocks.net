using Moq;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class DeleteUIDefinitionTests : DatabaseHelper
    {
        [Fact]
        public async Task ExecuteChecksInitialState()
        {
            var command = new DeleteUIDefinition
            {
                Name = "angular"
            };
            var engine = new FakeEngine();
            var result = await engine.ExecuteAsync(command);
            Assert.False(result.WasSuccessful);
            Assert.Equal("Unexpected error: Command is not in a valid state. Need to call either ValidateAsync or RestoreAsync", result.Error);
        }

        [Fact]
        public async Task ExecuteDeletesDefinition()
        {
            var command = new DeleteUIDefinition
            {
                Name = "angular"
            };
            using var store = InitialiseDatabase(new UIDefinition { Name = "angular" });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful);
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var definition = verifySession.Query<UIDefinition>().FirstOrDefault();
            Assert.Null(definition);
        }

        [Fact]
        public async Task RestoreFailsIfDefinitionIsMissing()
        {
            var command = new DeleteUIDefinition
            {
                Name = "angular"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Equal(new[] { "Definition 'angular' does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreReloadsDefinition()
        {
            var command = new DeleteUIDefinition
            {
                Name = "angular"
            };
            using var store = InitialiseDatabase(new UIDefinition { Name = "angular" });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateChecksForExistingDefinition()
        {
            var command = new DeleteUIDefinition
            {
                Name = "angular"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Definition 'angular' does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new DeleteUIDefinition
            {
                Name = "angular"
            };
            using var store = InitialiseDatabase(new UIDefinition { Name = "angular" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new DeleteUIDefinition();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Definition name is required" }, FakeEngine.GetErrors(errors));
        }
    }
}