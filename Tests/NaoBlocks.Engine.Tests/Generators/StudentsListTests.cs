using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Generators;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Generators
{
    public class StudentsListTests : DatabaseHelper
    {
        [Theory]
        [InlineData(ReportFormat.Unknown, false)]
        [InlineData(ReportFormat.Zip, false)]
        [InlineData(ReportFormat.Pdf, true)]
        [InlineData(ReportFormat.Excel, true)]
        [InlineData(ReportFormat.Text, true)]
        [InlineData(ReportFormat.Csv, true)]
        public void IsFormatAvailableChecksAllowedTypes(ReportFormat format, bool allowed)
        {
            var generator = new StudentsList();
            Assert.Equal(allowed, generator.IsFormatAvailable(format));
        }

        [Theory]
        [InlineData(ReportFormat.Csv, "Student-List.csv")]
        [InlineData(ReportFormat.Text, "Student-List.txt")]
        [InlineData(ReportFormat.Excel, "Student-List.xlsx")]
        public async Task GenerateAsyncHandlesDifferentFormats(ReportFormat format, string expectedName)
        {
            using var store = InitialiseDatabase(
                new User { Name = "Mia", Role = UserRole.Student, WhenAdded = new DateTime(2021, 3, 4) });
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<StudentsList>(session);

            // Act
            var output = await generator.GenerateAsync(format);

            // Assert
            Assert.Equal(expectedName, output.Item2);
            Assert.True(output.Item1.Length > 0);
        }

        [Fact]
        public async Task GenerateAsyncGeneratesReport()
        {
            // Arrange
            var student1 = new User
            {
                Name = "Moana",
                Role = UserRole.Student,
                WhenAdded = new DateTime(2021, 5, 6),
                Settings = new UserSettings
                {
                    RobotType = "Karetao",
                    Conditionals = true
                }
            };
            var student2 = new User
            {
                Name = "Ari",
                Role = UserRole.Student,
                WhenAdded = new DateTime(2021, 7, 8),
                Settings = new UserSettings
                {
                    RobotType = "Karetao",
                    Dances = true,
                    Loops = true,
                    Simple = true
                }
            };
            using var store = InitialiseDatabase(
                student1,
                student2,
                new User { Name = "Mia", Role = UserRole.Student, WhenAdded = new DateTime(2021, 3, 4) },
                new User { Name = "Taika", Role = UserRole.Teacher, WhenAdded = new DateTime(2021, 3, 4) });
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<StudentsList>(session);

            // Act
            var output = await generator.GenerateAsync(ReportFormat.Text);

            // Assert
            Assert.Equal("Student-List.txt", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            Assert.Equal(
                    @"Students
========
Name,Robot,When Added,Mode,Dances,Conditionals,Loops,Sensors,Variables,Age,Gender
Ari,Karetao,8-Jul-2021,Simple,Yes,No,Yes,No,No,,
Mia,,4-Mar-2021,,,,,,,,
Moana,Karetao,6-May-2021,Default,No,Yes,No,No,No,,
",
                text);
        }
    }
}