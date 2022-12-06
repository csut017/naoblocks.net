using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class ParseRobotTypeImportTests : DatabaseHelper
    {
        [Fact]
        public async Task ExecuteAsyncChecksForDusplicateName()
        {
            // Arrange
            var command = new ParseRobotTypeImport();
            PopulateData(command, "{\"name\":\"nao\"}");
            using var store = InitialiseDatabase(new RobotType { Name = "Nao" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            var output = result.As<RobotType>().Output;
            Assert.True(output?.IsDuplicate, "IsDuplicate not set");
        }

        [Fact]
        public async Task ExecuteAsyncChecksForName()
        {
            // Arrange
            var command = new ParseRobotTypeImport
            {
                SkipValidation = true
            };
            PopulateData(command, "{}");
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            var output = result.As<RobotType>().Output;
            Assert.Contains(
                "Property 'name' not set",
                output?.Message);
        }

        [Fact]
        public async Task ExecuteAsyncHandlesInvalidAllowDirectLogging()
        {
            // Arrange
            var command = new ParseRobotTypeImport
            {
                SkipValidation = true
            };
            PopulateData(command, "{\"directLogging\":\"rubbish\"}");
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            var output = result.As<RobotType>().Output;
            Assert.Contains(
                "Property 'directLogging' is not a valid boolean (true or false)",
                output?.Message);
        }

        [Fact]
        public async Task ExecuteAsyncHandlesInvalidIsDefault()
        {
            // Arrange
            var command = new ParseRobotTypeImport
            {
                SkipValidation = true
            };
            PopulateData(command, "{\"isDefault\":\"rubbish\"}");
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            var output = result.As<RobotType>().Output;
            Assert.Contains(
                "Property 'isDefault' is not a valid boolean (true or false)",
                output?.Message);
        }

        [Fact]
        public async Task ExecuteAsyncParsesAllowDirectLogging()
        {
            // Arrange
            var command = new ParseRobotTypeImport
            {
                SkipValidation = true
            };
            PopulateData(command, "{\"directLogging\":true}");
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            Assert.True(result.As<RobotType>().Output?.AllowDirectLogging, "AllowDirectLogging not set");
        }

        [Fact]
        public async Task ExecuteAsyncParsesIsDefault()
        {
            // Arrange
            var command = new ParseRobotTypeImport
            {
                SkipValidation = true
            };
            PopulateData(command, "{\"isDefault\":true}");
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            Assert.True(result.As<RobotType>().Output?.IsDefault, "IsDefault not set");
        }

        [Fact]
        public async Task ExecuteAsyncParsesName()
        {
            // Arrange
            var command = new ParseRobotTypeImport
            {
                SkipValidation = true
            };
            PopulateData(command, "{\"name\":\"New robot type\"}");
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            Assert.Equal(
                "New robot type",
                result.As<RobotType>().Output?.Name);
        }

        [Fact]
        public async Task ValidateAsyncChecksForArray()
        {
            // Arrange
            var command = new ParseRobotTypeImport();
            PopulateData(command, "[]");
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);

            // Assert
            Assert.Equal(
                new[] { "Unable to parse Robot Type definition: Root level definition should be a single definition" },
                FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateAsyncChecksForData()
        {
            // Arrange
            var command = new ParseRobotTypeImport();
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);

            // Assert
            Assert.Equal(
                new[] { "Data is required" },
                FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateAsyncHandlesNonJson()
        {
            // Arrange
            var command = new ParseRobotTypeImport();
            PopulateData(command, "This is a text file");

            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);

            // Assert
            Assert.Equal(
                new[] { "Unable to parse Robot Type definition: Definition is invalid JSON" },
                FakeEngine.GetErrors(errors));
        }

        private static void PopulateData(ParseRobotTypeImport command, string json)
        {
            command.Data = new MemoryStream(
                Encoding.UTF8.GetBytes(json));
        }
    }
}