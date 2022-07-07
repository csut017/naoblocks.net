using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Generators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Generators
{
    public class RobotTypeToolboxTests
         : DatabaseHelper
    {
        [Fact]
        public async Task GenerateAsyncErrorsWithMissingArgs()
        {
            // Arrange
            var robotType = new RobotType { Name = "Karetao" };
            var toolbox = new Toolbox { Name = "Testing" };
            robotType.Toolboxes.Add(toolbox);
            using var store = InitialiseDatabase(robotType);
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<RobotTypeToolbox>(session);

            // Act
            var error = await Assert.ThrowsAsync<Exception>(
                async () => await generator.GenerateAsync(ReportFormat.Xml, robotType));

            // Assert
            Assert.Equal("Unknown toolbox unknown", error.Message);
        }

        [Fact]
        public async Task GenerateAsyncErrorsWithUnknownToolbox()
        {
            // Arrange
            var robotType = new RobotType { Name = "Karetao" };
            var toolbox = new Toolbox { Name = "Testing" };
            robotType.Toolboxes.Add(toolbox);
            using var store = InitialiseDatabase(robotType);
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<RobotTypeToolbox>(session);
            generator.UseArguments(new Dictionary<string, string> {
                {"toolbox", "missing" }
            });

            // Act
            var error = await Assert.ThrowsAsync<Exception>(
                async () => await generator.GenerateAsync(ReportFormat.Xml, robotType));

            // Assert
            Assert.Equal("Unknown toolbox missing", error.Message);
        }

        [Fact]
        public async Task GenerateAsyncGeneratesXml()
        {
            // Arrange
            var robotType = new RobotType { Name = "Karetao" };
            var toolbox = new Toolbox { Name = "Testing" };
            robotType.Toolboxes.Add(toolbox);
            using var store = InitialiseDatabase(robotType);
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<RobotTypeToolbox>(session);
            generator.UseArguments(new Dictionary<string, string> {
                {"toolbox", "Testing" }
            });

            // Act
            var output = await generator.GenerateAsync(ReportFormat.Xml, robotType);

            // Assert
            Assert.Equal("Karetao-toolbox.xml", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            Assert.Equal("<toolbox />", text);
        }

        [Theory]
        [ReportFormatData(ReportFormat.Xml)]
        public void IsFormatAvailableChecksAllowedTypes(ReportFormat format, bool allowed)
        {
            var generator = new RobotTypeToolbox();
            Assert.Equal(allowed, generator.IsFormatAvailable(format));
        }
    }
}