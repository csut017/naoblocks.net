using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Generators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Generators
{
    public class AllListsTests : DatabaseHelper
    {
        [Fact]
        public async Task GenerateAsyncGeneratesAllReport()
        {
            // Arrange
            var type1 = new RobotType { Id = "types/1", Name = "Karetao", IsDefault = true, WhenAdded = new DateTime(2021, 1, 7) };
            type1.Toolboxes.Add(new Toolbox());
            using var store = InitialiseDatabase(
                new Robot { RobotTypeId = "types/1", MachineName = "Mihīni-2", FriendlyName = "Taita", IsInitialised = true, WhenAdded = new DateTime(2021, 12, 11) },
                new Robot { RobotTypeId = "types/2", MachineName = "Mihīni-1", FriendlyName = "Mia", IsInitialised = false, WhenAdded = new DateTime(2021, 10, 9) },
                GenerateStudent("Tane", "Nao", new DateTime(2021, 2, 3), "Default", null, null, 0, 0, null),
                GenerateStudent("Wahine", "Nao", new DateTime(2021, 5, 6), "Default", 5, "Female", 1, 1, "Mihīni"),
                GenerateStudent("Rangatahi", "Nao", new DateTime(2021, 8, 9), "Default", 16, null, 0, 2, "Mihīni"),
                type1,
                new RobotType { Id = "types/2", Name = "Nao", IsDefault = false, WhenAdded = new DateTime(2021, 2, 17) });
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<AllLists>(session);
            generator.UseArguments(new Dictionary<string, string>
            {
                { "robots", "yes" },
                { "students", "yes" },
                { "types", "yes" },
            });

            // Act
            var output = await generator.GenerateAsync(ReportFormat.Text);

            // Assert
            Assert.Equal("All-Lists.txt", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            Assert.Equal(
                    @"Robots
======
Machine Name,Friendly Name,Type,When Added,Initialized
Mihīni-1,Mia,Nao,2021-10-09,No
Mihīni-2,Taita,Karetao,2021-12-11,Yes
Students
========
Name,Robot Type,When Added,Toolbox,Age,Gender,View Mode,Allocation Mode,Allocated Robot
Rangatahi,Nao,2021-08-09,Default,16,,Blocks,Prefer,Mihīni
Tane,Nao,2021-02-03,Default,,,Blocks,Any,
Wahine,Nao,2021-05-06,Default,5,Female,Tangibles,Require,Mihīni
Robot Types
===========
Name,Is Default,# Toolboxes,When Added
Karetao,Yes,1,2021-01-07
Nao,No,0,2021-02-17
",
                text);
        }

        [Fact]
        public async Task GenerateAsyncGeneratesRobotsReport()
        {
            // Arrange
            using var store = InitialiseDatabase(
                new Robot { RobotTypeId = "types/1", MachineName = "Mihīni-2", FriendlyName = "Taita", IsInitialised = true, WhenAdded = new DateTime(2021, 12, 11) },
                new Robot { RobotTypeId = "types/2", MachineName = "Mihīni-1", FriendlyName = "Mia", IsInitialised = false, WhenAdded = new DateTime(2021, 10, 9) },
                new RobotType { Id = "types/1", Name = "Karetao" });
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<AllLists>(session);
            generator.UseArguments(new Dictionary<string, string>
            {
                { "robots", "yes" },
            });

            // Act
            var output = await generator.GenerateAsync(ReportFormat.Text);

            // Assert
            Assert.Equal("All-Lists.txt", output.Item2);
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

        [Fact]
        public async Task GenerateAsyncGeneratesStudentsReport()
        {
            // Arrange
            using var store = InitialiseDatabase(
                GenerateStudent("Tane", "Nao", new DateTime(2021, 2, 3), "Default", null, null, 0, 0, null),
                GenerateStudent("Wahine", "Nao", new DateTime(2021, 5, 6), "Default", 5, "Female", 1, 1, "Mihīni"),
                GenerateStudent("Rangatahi", "Nao", new DateTime(2021, 8, 9), "Default", 16, null, 0, 2, "Mihīni"));
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<AllLists>(session);
            generator.UseArguments(new Dictionary<string, string>
            {
                { "students", "yes" },
            });

            // Act
            var output = await generator.GenerateAsync(ReportFormat.Text);

            // Assert
            Assert.Equal("All-Lists.txt", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            Assert.Equal(
                    @"Students
========
Name,Robot Type,When Added,Toolbox,Age,Gender,View Mode,Allocation Mode,Allocated Robot
Rangatahi,Nao,2021-08-09,Default,16,,Blocks,Prefer,Mihīni
Tane,Nao,2021-02-03,Default,,,Blocks,Any,
Wahine,Nao,2021-05-06,Default,5,Female,Tangibles,Require,Mihīni
",
                text);
        }

        [Fact]
        public async Task GenerateAsyncGeneratesTypesReport()
        {
            // Arrange
            var type1 = new RobotType { Id = "types/1", Name = "Karetao", IsDefault = true, WhenAdded = new DateTime(2021, 1, 7) };
            type1.Toolboxes.Add(new Toolbox());
            using var store = InitialiseDatabase(
                type1,
                new RobotType { Id = "types/2", Name = "Mihīni", IsDefault = false, WhenAdded = new DateTime(2021, 2, 17) });
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<AllLists>(session);
            generator.UseArguments(new Dictionary<string, string>
            {
                { "types", "yes" },
            });

            // Act
            var output = await generator.GenerateAsync(ReportFormat.Text);

            // Assert
            Assert.Equal("All-Lists.txt", output.Item2);
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
        [InlineData(ReportFormat.Csv, "All-Lists.csv")]
        [InlineData(ReportFormat.Text, "All-Lists.txt")]
        [InlineData(ReportFormat.Excel, "All-Lists.xlsx")]
        public async Task GenerateAsyncHandlesDifferentFormats(ReportFormat format, string expectedName)
        {
            using var store = InitialiseDatabase(
                new Robot { RobotTypeId = "types/1", MachineName = "Mihīni-1", FriendlyName = "Taita", IsInitialised = true },
                new Robot { RobotTypeId = "types/2", MachineName = "Mihīni-2", FriendlyName = "Mia", IsInitialised = false },
                new RobotType { Id = "types/1", Name = "Karetao" });
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<AllLists>(session);
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
            var generator = new AllLists();
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