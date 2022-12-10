using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Generators;
using NaoBlocks.Engine.Indices;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Generators
{
    public class RobotTypeDefinitionTests
            : DatabaseHelper
    {
        [Fact]
        public async Task GenerateAsyncGeneratesReport()
        {
            // Arrange
            var robotType = new RobotType { Id = "types/1", Name = "Nao", AllowDirectLogging = true };
            var toolbox = new Toolbox { Name = "Default", IsDefault = true };
            robotType.Toolboxes.Add(toolbox);
            toolbox.Categories.Add(new ToolboxCategory { Name = "Category #1" });
            toolbox.Categories.Add(new ToolboxCategory { Name = "Category #2" });
            robotType.Toolboxes.Add(new Toolbox { Name = "Secondary", IsDefault = false });
            robotType.CustomValues.Add(new NamedValue { Name = "One", Value = "Tahi" });
            robotType.CustomValues.Add(new NamedValue { Name = "Two", Value = "Rua" });
            robotType.LoggingTemplates.Add(GenerateTemplate("first", "First", ClientMessageType.RobotAction, "some value"));
            robotType.LoggingTemplates.Add(GenerateTemplate("second", "Second action", ClientMessageType.RobotDebugMessage, "another", "value"));
            using var store = InitialiseDatabase(
                robotType);
            await this.InitialiseIndicesAsync(store, new RobotLogByRobotTypeId());
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<RobotTypeDefinition>(session);

            // Act
            var output = await generator.GenerateAsync(ReportFormat.Json, robotType);

            // Assert
            Assert.Equal("Robot-Type-Definition-Nao.json", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            var toolboxXml = toolbox.ExportToXml(Toolbox.Format.Toolbox)!
                .Replace("<", "\\u003C")
                .Replace(">", "\\u003E")
                .Replace("\"", "\\u0022");
            var expected = "{" +
                "\"name\":\"Nao\",\"isDefault\":false,\"directLogging\":true," +
                "\"toolboxes\":[" + "" +
                    "{\"name\":\"Default\",\"isDefault\":true,\"definition\":\"" + toolboxXml + "\"}," +
                    "{\"name\":\"Secondary\",\"isDefault\":false,\"definition\":\"\\u003Ctoolbox useEvents=\\u0022no\\u0022 /\\u003E\"}" +
                "]," +
                "\"values\":[{\"name\":\"One\",\"value\":\"Tahi\"},{\"name\":\"Two\",\"value\":\"Rua\"}]," +
                "\"templates\":[" +
                    "{\"category\":\"first\",\"text\":\"First\",\"type\":\"RobotAction\",\"values\":\"some value\"}," +
                    "{\"category\":\"second\",\"text\":\"Second action\",\"type\":\"RobotDebugMessage\",\"values\":\"another,value\"}" +
                "]" +
                "}";
            Assert.Equal(expected, text);
        }

        [Fact]
        public async Task GenerateAsyncIncludesRobots()
        {
            // Arrange
            var robotType = new RobotType { Id = "types/1", Name = "Nao", AllowDirectLogging = true };
            var toolbox = new Toolbox { Name = "Default", IsDefault = true };
            robotType.Toolboxes.Add(toolbox);
            toolbox.Categories.Add(new ToolboxCategory { Name = "Category #1" });
            toolbox.Categories.Add(new ToolboxCategory { Name = "Category #2" });
            robotType.Toolboxes.Add(new Toolbox { Name = "Secondary", IsDefault = false });
            robotType.CustomValues.Add(new NamedValue { Name = "One", Value = "Tahi" });
            robotType.CustomValues.Add(new NamedValue { Name = "Two", Value = "Rua" });
            robotType.LoggingTemplates.Add(GenerateTemplate("first", "First", ClientMessageType.RobotAction, "some value"));
            robotType.LoggingTemplates.Add(GenerateTemplate("second", "Second action", ClientMessageType.RobotDebugMessage, "another", "value"));
            var robot = new Robot { RobotTypeId = "types/1", MachineName = "Mihīni-2", FriendlyName = "Taita" };
            robot.CustomValues.Add(NamedValue.New("Three", "Toru"));
            using var store = InitialiseDatabase(
                robotType,
                robot);
            await this.InitialiseIndicesAsync(store, new RobotLogByRobotTypeId());
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<RobotTypeDefinition>(session);

            // Act
            generator.UseArguments(new Dictionary<string, string>
            {
                { "robots", "yes" }
            });
            var output = await generator.GenerateAsync(ReportFormat.Json, robotType);

            // Assert
            Assert.Equal("Robot-Type-Definition-Nao.json", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            var toolboxXml = toolbox.ExportToXml(Toolbox.Format.Toolbox)!
                .Replace("<", "\\u003C")
                .Replace(">", "\\u003E")
                .Replace("\"", "\\u0022");
            var expected = "{" +
                "\"name\":\"Nao\",\"isDefault\":false,\"directLogging\":true," +
                "\"toolboxes\":[" + "" +
                    "{\"name\":\"Default\",\"isDefault\":true,\"definition\":\"" + toolboxXml + "\"}," +
                    "{\"name\":\"Secondary\",\"isDefault\":false,\"definition\":\"\\u003Ctoolbox useEvents=\\u0022no\\u0022 /\\u003E\"}" +
                "]," +
                "\"values\":[{\"name\":\"One\",\"value\":\"Tahi\"},{\"name\":\"Two\",\"value\":\"Rua\"}]," +
                "\"templates\":[" +
                    "{\"category\":\"first\",\"text\":\"First\",\"type\":\"RobotAction\",\"values\":\"some value\"}," +
                    "{\"category\":\"second\",\"text\":\"Second action\",\"type\":\"RobotDebugMessage\",\"values\":\"another,value\"}" +
                "]," +
                "\"robots\":[" +
                    "{\"machineName\":\"Mih\\u012Bni-2\",\"friendlyName\":\"Taita\",\"values\":[{\"name\":\"Three\",\"value\":\"Toru\"}]}" +
                "]" +
            "}";
            Assert.Equal(expected, text);
        }

        [Theory]
        [ReportFormatData(ReportFormat.Json)]
        public void IsFormatAvailableChecksAllowedTypes(ReportFormat format, bool allowed)
        {
            var generator = new RobotTypeDefinition();
            Assert.Equal(allowed, generator.IsFormatAvailable(format));
        }

        private static LoggingTemplate GenerateTemplate(string category, string text, ClientMessageType type, params string[] values)
        {
            return new LoggingTemplate
            {
                Category = category,
                Text = text,
                MessageType = type,
                ValueNames = values
            };
        }
    }
}