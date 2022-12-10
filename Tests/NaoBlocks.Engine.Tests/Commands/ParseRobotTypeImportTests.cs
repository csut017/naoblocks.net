using NaoBlocks.Common;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class ParseRobotTypeImportTests : DatabaseHelper
    {
        [Fact]
        public async Task ExecuteAsyncAddsRobot()
        {
            // Arrange
            var command = new ParseRobotTypeImport
            {
                SkipValidation = true
            };
            PopulateData(command, "{\"robots\":[{\"machineName\": \"Kiwikiwi\",\"friendlyName\": \"Grey\"}]}");
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            var output = result.As<RobotTypeImport>().Output?.RobotType;
            Assert.Equal(
                new[] { "First" },
                output?.CustomValues.Select(cv => cv.Name).ToArray());
            Assert.Equal(
                new[] { "Tahi" },
                output?.CustomValues.Select(cv => cv.Value).ToArray());
        }

        [Fact]
        public async Task ExecuteAsyncAddsTemplate()
        {
            // Arrange
            var command = new ParseRobotTypeImport
            {
                SkipValidation = true
            };
            PopulateData(command, "{\"templates\":[{\"category\":\"First\",\"text\":\"Tahi\",\"type\":\"RobotAction\",\"values\":\"One,Two\"}]}");
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            var output = result.As<RobotTypeImport>().Output?.RobotType;
            Assert.Equal(
                new[] { "First" },
                output?.LoggingTemplates.Select(lt => lt.Category).ToArray());
            Assert.Equal(
                new[] { "Tahi" },
                output?.LoggingTemplates.Select(lt => lt.Text).ToArray());
            Assert.Equal(
                new[] { ClientMessageType.RobotAction },
                output?.LoggingTemplates.Select(lt => lt.MessageType).ToArray());
            Assert.Equal(
                new[] { "One", "Two" },
                output?.LoggingTemplates.SelectMany(lt => lt.ValueNames).ToArray());
        }

        [Fact]
        public async Task ExecuteAsyncAddsToolbox()
        {
            // Arrange
            var command = new ParseRobotTypeImport
            {
                SkipValidation = true
            };
            PopulateData(command, "{\"toolboxes\":[{\"name\":\"First\",\"isDefault\":true,\"definition\":\"\\u003Ctoolbox\\u003E\u003C/toolbox\u003E\"}]}");
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            var output = result.As<RobotTypeImport>().Output?.RobotType;
            Assert.Equal(
                new[] { "First" },
                output?.Toolboxes.Select(tb => tb.Name).ToArray());
            Assert.Equal(
                new[] { true },
                output?.Toolboxes.Select(tb => tb.IsDefault).ToArray());
            Assert.Equal(
                new[] { "<toolbox></toolbox>" },
                output?.Toolboxes.Select(tb => tb.RawXml).ToArray());
        }

        [Fact]
        public async Task ExecuteAsyncAddsValue()
        {
            // Arrange
            var command = new ParseRobotTypeImport
            {
                SkipValidation = true
            };
            PopulateData(command, "{\"values\":[{\"name\":\"First\",\"value\":\"Tahi\"}]}");
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            var output = result.As<RobotTypeImport>().Output?.RobotType;
            Assert.Equal(
                new[] { "First" },
                output?.CustomValues.Select(cv => cv.Name).ToArray());
            Assert.Equal(
                new[] { "Tahi" },
                output?.CustomValues.Select(cv => cv.Value).ToArray());
        }

        [Fact]
        public async Task ExecuteAsyncChecksForDuplicateName()
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
            var output = result.As<RobotTypeImport>().Output;
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
            var output = result.As<RobotTypeImport>().Output?.RobotType;
            Assert.Contains(
                "Property 'name' not set",
                output?.Message);
        }

        [Fact]
        public async Task ExecuteAsyncChecksForToolboxName()
        {
            // Arrange
            var command = new ParseRobotTypeImport
            {
                SkipValidation = true
            };
            PopulateData(command, "{\"toolboxes\":[{}]}");
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            var output = result.As<RobotTypeImport>().Output?.RobotType;
            Assert.Contains(
                "Property 'name' not set for toolbox #1",
                output?.Message);
        }

        [Fact]
        public async Task ExecuteAsyncChecksTemplatesIsArray()
        {
            // Arrange
            var command = new ParseRobotTypeImport
            {
                SkipValidation = true
            };
            PopulateData(command, "{\"templates\":{}}");
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            var output = result.As<RobotTypeImport>().Output?.RobotType;
            Assert.Contains(
                "Property 'templates' is not a valid array",
                output?.Message);
        }

        [Fact]
        public async Task ExecuteAsyncChecksTemplateType()
        {
            // Arrange
            var command = new ParseRobotTypeImport
            {
                SkipValidation = true
            };
            PopulateData(command, "{\"templates\":[{\"category\":\"First\",\"text\":\"Tahi\",\"type\":\"Rubbish\",\"values\":\"One,Two\"}]}");
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            var output = result.As<RobotTypeImport>().Output?.RobotType;
            Assert.Equal(
                new[] { "First" },
                output?.LoggingTemplates.Select(lt => lt.Category).ToArray());
            Assert.Equal(
                new[] { "Tahi" },
                output?.LoggingTemplates.Select(lt => lt.Text).ToArray());
            Assert.Equal(
                new[] { ClientMessageType.Unknown },
                output?.LoggingTemplates.Select(lt => lt.MessageType).ToArray());
            Assert.Equal(
                new[] { "One", "Two" },
                output?.LoggingTemplates.SelectMany(lt => lt.ValueNames).ToArray());
            Assert.Contains(
                "Property 'type' is not a valid ClientMessageType value for template #1",
                output?.Message);
        }

        [Fact]
        public async Task ExecuteAsyncChecksToolboxesIsArray()
        {
            // Arrange
            var command = new ParseRobotTypeImport
            {
                SkipValidation = true
            };
            PopulateData(command, "{\"toolboxes\":{}}");
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            var output = result.As<RobotTypeImport>().Output?.RobotType;
            Assert.Contains(
                "Property 'toolboxes' is not a valid array",
                output?.Message);
        }

        [Fact]
        public async Task ExecuteAsyncChecksValuesIsArray()
        {
            // Arrange
            var command = new ParseRobotTypeImport
            {
                SkipValidation = true
            };
            PopulateData(command, "{\"values\":{}}");
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            var output = result.As<RobotTypeImport>().Output?.RobotType;
            Assert.Contains(
                "Property 'values' is not a valid array",
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
            var output = result.As<RobotTypeImport>().Output?.RobotType;
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
            var output = result.As<RobotTypeImport>().Output?.RobotType;
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
            Assert.True(result.As<RobotTypeImport>().Output?.RobotType?.AllowDirectLogging, "AllowDirectLogging not set");
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
            Assert.True(result.As<RobotTypeImport>().Output?.RobotType?.IsDefault, "IsDefault not set");
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
                result.As<RobotTypeImport>().Output?.RobotType?.Name);
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