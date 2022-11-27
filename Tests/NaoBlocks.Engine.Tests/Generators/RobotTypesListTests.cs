using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Generators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Generators
{
    public class RobotTypesListTests : DatabaseHelper
    {
        [Fact]
        public async Task GenerateAsyncGeneratesReport()
        {
            // Arrange
            var type1 = new RobotType { Id = "types/1", Name = "Karetao", IsDefault = true, WhenAdded = new DateTime(2021, 1, 7) };
            type1.Toolboxes.Add(new Toolbox());
            using var store = InitialiseDatabase(
                type1,
                new RobotType { Id = "types/2", Name = "Mihīni", IsDefault = false, WhenAdded = new DateTime(2021, 2, 17) });
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<RobotTypesList>(session);
            generator.UseArguments(new Dictionary<string, string>
            {
                { "types", "yes" },
            });

            // Act
            var output = await generator.GenerateAsync(ReportFormat.Text);

            // Assert
            Assert.Equal("Robot-Type-List.txt", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            Assert.Equal(
                    @"Robot Types
===========
Name,Is Default,# Toolboxes,When Added
Karetao,Yes,1,2021-01-07
Mihīni,No,0,2021-02-17
",
                text);
        }

        [Theory]
        [InlineData(ReportFormat.Csv, "Robot-Type-List.csv")]
        [InlineData(ReportFormat.Text, "Robot-Type-List.txt")]
        [InlineData(ReportFormat.Excel, "Robot-Type-List.xlsx")]
        public async Task GenerateAsyncHandlesDifferentFormats(ReportFormat format, string expectedName)
        {
            using var store = InitialiseDatabase(
                new Robot { RobotTypeId = "types/1", MachineName = "Mihīni-1", FriendlyName = "Taita", IsInitialised = true },
                new Robot { RobotTypeId = "types/2", MachineName = "Mihīni-2", FriendlyName = "Mia", IsInitialised = false },
                new RobotType { Id = "types/1", Name = "Karetao" });
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<RobotTypesList>(session);
            generator.UseArguments(new Dictionary<string, string>
            {
                { "robots", "yes" },
            });

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
            var generator = new RobotTypesList();
            Assert.Equal(allowed, generator.IsFormatAvailable(format));
        }

        private static User GenerateStudent(string name, string robotType, DateTime whenAdded, string toolbox, int? age, string? gender, int viewMode, int allocationMode, string? allocatedRobot)
        {
            var student = new User { Name = name, WhenAdded = whenAdded };
            student.Settings.RobotType = robotType;
            student.Settings.Toolbox = toolbox;
            student.Settings.ViewMode = viewMode;
            student.Settings.AllocationMode = allocationMode;
            student.Settings.RobotId = allocatedRobot;
            if (age.HasValue || !string.IsNullOrEmpty(gender))
            {
                student.StudentDetails = new StudentDetails
                {
                    Age = age,
                    Gender = gender
                };
            }
            return student;
        }
    }
}