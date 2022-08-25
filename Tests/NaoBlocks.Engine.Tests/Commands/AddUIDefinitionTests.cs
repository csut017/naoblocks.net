using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class AddUIDefinitionTests : DatabaseHelper
    {
        [Fact]
        public async Task ExecuteAddsDefinition()
        {
            var command = new AddUIDefinition
            {
                Name = "angular",
                Definition = new FakeUIDefinition()
            };
            using var store = InitialiseDatabase();

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful);
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var definition = verifySession.Query<UIDefinition>().First();
            Assert.Equal("angular", definition?.Name);
            Assert.IsType<FakeUIDefinition>(definition?.Definition);
        }

        [Fact]
        public async Task ValidateChecksForExistingDefinition()
        {
            var command = new AddUIDefinition
            {
                Name = "angular",
                Definition = new FakeUIDefinition()
            };
            using var store = InitialiseDatabase(new UIDefinition { Name = "angular" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Definition already exists" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateChecksIgnoresExistingDefinition()
        {
            var command = new AddUIDefinition
            {
                Name = "angular",
                Definition = new FakeUIDefinition(),
                IgnoreExisting = true
            };
            using var store = InitialiseDatabase(new UIDefinition { Name = "angular" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateChecksIgnoresExistingDefinitionPlusFailsIfInvalidDefinition()
        {
            var definition = new FakeUIDefinition();
            definition.AddValidationError("Failed");
            var command = new AddUIDefinition
            {
                Name = "angular",
                Definition = definition,
                IgnoreExisting = true
            };
            using var store = InitialiseDatabase(new UIDefinition { Name = "angular" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(
                new[] { "Failed" },
                errors.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidateFailsIfDefinitionIsInvalid()
        {
            var definition = new FakeUIDefinition();
            definition.AddValidationError("Failed");
            var command = new AddUIDefinition
            {
                Name = "angular",
                Definition = definition
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(
                new[] { "Failed" },
                errors.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new AddUIDefinition
            {
                Name = "angular",
                Definition = new FakeUIDefinition()
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new AddUIDefinition();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Definition name is required", "Definition is required" }, FakeEngine.GetErrors(errors));
        }
    }
}