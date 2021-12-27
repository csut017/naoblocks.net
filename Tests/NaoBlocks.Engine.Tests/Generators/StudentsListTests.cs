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
                },
                StudentDetails = new StudentDetails
                {
                    Age = 10,
                    Gender = "Female"
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
Ari,Karetao,2021-07-08,Simple,Yes,No,Yes,No,No,,
Mia,,2021-03-04,,,,,,,,
Moana,Karetao,2021-05-06,Default,No,Yes,No,No,No,10,Female
",
                text);
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
    }
}