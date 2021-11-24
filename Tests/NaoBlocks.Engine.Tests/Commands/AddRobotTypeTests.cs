using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using Raven.TestDriver;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class AddRobotTypeTests : RavenTestDriver
    {
        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new AddRobotType();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Name is required for a robot type" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateChecksForExistingUser()
        {
            var command = new AddRobotType
            {
                Name = "Bobbot"
            };
            using var store = GetDocumentStore();
            using (var initSession = store.OpenSession())
            {
                initSession.Store(new RobotType { Name = "Bobbot" });
                initSession.SaveChanges();
            }
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Robot type with name Bobbot already exists" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new AddRobotType
            {
                Name = "Bobbot"
            };
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ExecuteAddsRobotType()
        {
            var command = new AddRobotType
            {
                Name = "Bobbot"
            };
            var engine = new FakeEngine();
            var result = await engine.ExecuteAsync(command);
            Assert.True(result.WasSuccessful);
            Assert.True(engine.DatabaseSession.StoreCalled);
            var user = Assert.IsType<RobotType>(engine.DatabaseSession.GetLastModifiedEntity());
            Assert.Equal("Bobbot", user.Name);
        }

        [Fact]
        public async Task ExecuteTrimsWhitespaceFromName()
        {
            var command = new AddRobotType
            {
                Name = " Bobbot "
            };
            var engine = new FakeEngine();
            await engine.ExecuteAsync(command);
            var user = Assert.IsType<RobotType>(engine.DatabaseSession.GetLastModifiedEntity());
            Assert.Equal("Bobbot", user.Name);
        }
    }
}
