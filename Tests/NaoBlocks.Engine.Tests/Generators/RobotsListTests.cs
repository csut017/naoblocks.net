using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Generators;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Generators
{
    public class RobotsListTests : DatabaseHelper
    {
        [Fact]
        public async Task GenerateAsyncGeneratesReport()
        {
            // Arrange
            using var store = InitialiseDatabase(
                new Robot { RobotTypeId = "types/1", MachineName = "Mihīni-2", FriendlyName = "Taita", IsInitialised = true, WhenAdded = new DateTime(2021, 12, 11) },
                new Robot { RobotTypeId = "types/2", MachineName = "Mihīni-1", FriendlyName = "Mia", IsInitialised = false, WhenAdded = new DateTime(2021, 10, 9) },
                new RobotType { Id = "types/1", Name = "Karetao" });
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<RobotsList>(session);

            // Act
            var output = await generator.GenerateAsync(ReportFormat.Text);

            // Assert
            Assert.Equal("Robot-List.txt", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            Assert.Equal(
                    @"Robots
======
Machine Name,Friendly Name,Type,When Added,Initialized
Mihīni-1,Mia,,2021-10-09,No
Mihīni-2,Taita,Karetao,2021-12-11,Yes
",
                text);
        }

        [Theory]
        [InlineData(ReportFormat.Csv, "Robot-List.csv")]
        [InlineData(ReportFormat.Text, "Robot-List.txt")]
        [InlineData(ReportFormat.Excel, "Robot-List.xlsx")]
        public async Task GenerateAsyncHandlesDifferentFormats(ReportFormat format, string expectedName)
        {
            using var store = InitialiseDatabase(
                new Robot { RobotTypeId = "types/1", MachineName = "Mihīni-1", FriendlyName = "Taita", IsInitialised = true },
                new Robot { RobotTypeId = "types/2", MachineName = "Mihīni-2", FriendlyName = "Mia", IsInitialised = false },
                new RobotType { Id = "types/1", Name = "Karetao" });
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<RobotsList>(session);

            // Act
            var output = await generator.GenerateAsync(format);

            // Assert
            Assert.Equal(expectedName, output.Item2);
            Assert.True(output.Item1.Length > 0);
        }

        [Theory]
        [ReportFormatData(ReportFormat.Excel, ReportFormat.Pdf, ReportFormat.Text, ReportFormat.Csv)]
        public void IsFormatAvailableChecksAllowedTypes(ReportFormat format, bool allowed)
        {
            var generator = new RobotsList();
            Assert.Equal(allowed, generator.IsFormatAvailable(format));
        }
    }
}