using NaoBlocks.Common;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class AddLoggingTemplateToRobotTypeTests
        : DatabaseHelper
    {
        [Fact]
        public async Task ExecuteAddsNewTemplate()
        {
            var command = new AddLoggingTemplateToRobotType
            {
                Category = "action",
                Text = "Some action",
                RobotTypeName = "Nao",
                MessageType = ClientMessageType.RobotAction,
                ValueNames = new[] { "One", "Tahi" },
            };
            using var store = InitialiseDatabase(
                new RobotType { Name = "Nao" });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var template = verifySession.Query<RobotType>().First().LoggingTemplates.FirstOrDefault();
            Assert.NotNull(template);
            Assert.Equal("action", template?.Category);
            Assert.Equal("Some action", template?.Text);
            Assert.Equal(ClientMessageType.RobotAction, template?.MessageType);
            Assert.Equal(new[] { "One", "Tahi" }, template?.ValueNames);
        }

        [Fact]
        public async Task ExecuteAddsNewTemplateWithOptionalPopulation()
        {
            var command = new AddLoggingTemplateToRobotType
            {
                Category = "action",
                RobotTypeName = "Nao",
                MessageType = ClientMessageType.RobotAction
            };
            using var store = InitialiseDatabase(
                new RobotType { Name = "Nao" });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.ValidateAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var template = verifySession.Query<RobotType>().First().LoggingTemplates.FirstOrDefault();
            Assert.NotNull(template);
            Assert.Equal("action", template?.Category);
            Assert.Equal("action", template?.Text);
            Assert.Equal(ClientMessageType.RobotAction, template?.MessageType);
            Assert.Equal(Array.Empty<string>(), template?.ValueNames);
        }

        [Fact]
        public async Task RestoreFailsIfRobotTypeIsMissing()
        {
            var command = new AddLoggingTemplateToRobotType
            {
                RobotTypeName = "Nao",
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Equal(new[] { "Robot Type 'Nao' does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreReloadsRobotType()
        {
            var command = new AddLoggingTemplateToRobotType
            {
                RobotTypeName = "Nao",
            };
            using var store = InitialiseDatabase(
                new RobotType { Name = "Nao" });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateChecksForDuplicateCategory()
        {
            var command = new AddLoggingTemplateToRobotType
            {
                Category = "action",
                RobotTypeName = "Nao",
                MessageType = ClientMessageType.RobotAction
            };
            var robotType = new RobotType { Name = "Nao" };
            robotType.LoggingTemplates.Add(
                new LoggingTemplate { Category = "action" });
            using var store = InitialiseDatabase(robotType);
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Category 'action' already exists" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateChecksForExistingRobotType()
        {
            var command = new AddLoggingTemplateToRobotType
            {
                Category = "action",
                RobotTypeName = "Nao",
                MessageType = ClientMessageType.RobotAction
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Robot Type 'Nao' does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new AddLoggingTemplateToRobotType
            {
                Category = "action",
                RobotTypeName = "Nao",
                MessageType = ClientMessageType.RobotAction
            };
            using var store = InitialiseDatabase(
                new RobotType { Name = "Nao" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidatePopulatesOptionalFields()
        {
            var command = new AddLoggingTemplateToRobotType
            {
                Category = "action",
                RobotTypeName = "Nao",
                MessageType = ClientMessageType.RobotAction
            };
            using var store = InitialiseDatabase(
                new RobotType { Name = "Nao" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal("action", command.Text);
            Assert.Equal(Array.Empty<string>(), command.ValueNames);
        }

        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new AddLoggingTemplateToRobotType();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Category is required", "Robot Type Name is required", "Message Type is required" }, FakeEngine.GetErrors(errors));
        }
    }
}