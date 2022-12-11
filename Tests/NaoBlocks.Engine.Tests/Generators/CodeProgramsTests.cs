using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Generators;
using NaoBlocks.Engine.Indices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Generators
{
    public class CodeProgramsTests
        : DatabaseHelper
    {
        [Fact]
        public async Task GenerateAsyncGeneratesReport()
        {
            // Arrange
            var user = new User { Id = "users/1", Name = "Mia" };
            var program1 = new CodeProgram { Code = "go()", Name = "None", Number = 1, UserId = user.Id, WhenAdded = DateTime.Now.AddDays(-3) };
            var program2 = new CodeProgram { Code = "reset()", Number = 2, UserId = user.Id, WhenAdded = DateTime.Now.AddDays(-2), Source = "TopCodes" };
            var program3 = new CodeProgram { Code = "start()", Number = 3, UserId = user.Id, WhenAdded = DateTime.Now.AddDays(-1), Source = "Blockly" };
            using var store = InitialiseDatabase(
                user,
                program1,
                program2,
                program3);
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<CodePrograms>(session);

            // Act
            var output = await generator.GenerateAsync(ReportFormat.Text, user);

            // Assert
            Assert.Equal("Mia-programs.txt", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            Assert.Matches(
                    @"Logs
====
Source,Number,Name,When Added,Code
Blockly,3,,\d{4}-\d{2}-\d{2},start\(\)
TopCodes,2,,\d{4}-\d{2}-\d{2},reset\(\)
,1,None,\d{4}-\d{2}-\d{2},go\(\)
",
                text);
        }

        [Fact]
        public async Task GenerateAsyncGeneratesReportWithDateRange()
        {
            // Arrange
            var user = new User { Id = "users/1", Name = "Mia" };
            var program1 = new CodeProgram { Code = "go()", Name = "None", Number = 1, UserId = user.Id, WhenAdded = DefaultTestDateTime.AddDays(-1) };
            var program2 = new CodeProgram { Code = "reset()", Number = 2, UserId = user.Id, WhenAdded = DefaultTestDateTime };
            var program3 = new CodeProgram { Code = "start()", Number = 3, UserId = user.Id, WhenAdded = DateTime.Now };
            using var store = InitialiseDatabase(
                user,
                program1,
                program2,
                program3);
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<CodePrograms>(session);

            // Act
            generator.UseArguments(new Dictionary<string, string>
            {
                {"from", "2021-03-01" },
                {"to", "2021-03-30" },
            });
            var output = await generator.GenerateAsync(ReportFormat.Text, user);

            // Assert
            Assert.Equal("Mia-programs.txt", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            Assert.Equal(
                    @"Logs
====
Source,Number,Name,When Added,Code
,2,,2021-03-04,reset()
,1,None,2021-03-03,go()
",
                text);
        }

        [Fact]
        public async Task GenerateAsyncGeneratesSystemReport()
        {
            // Arrange
            var user1 = new User { Id = "users/1", Name = "Mia" };
            var user2 = new User { Id = "users/2", Name = "Mateo" };
            var program1 = new CodeProgram { Code = "go()", Name = "None", Number = 1, UserId = user1.Id, WhenAdded = DefaultTestDateTime.AddDays(-1) };
            var program2 = new CodeProgram { Code = "reset()", Number = 2, UserId = user1.Id, WhenAdded = DefaultTestDateTime };
            var program3 = new CodeProgram { Code = "start()", Number = 1, UserId = user2.Id, WhenAdded = DefaultTestDateTime.AddDays(-2) };
            using var store = InitialiseDatabase(
                user1,
                user2,
                program1,
                program2,
                program3);
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<CodePrograms>(session);

            // Act
            generator.UseArguments(new Dictionary<string, string>
            {
                {"from", "2021-03-01" },
                {"to", "2021-03-30" },
            });
            var output = await generator.GenerateAsync(ReportFormat.Text);

            // Assert
            Assert.Equal("All-programs.txt", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            Assert.Equal(
                    @"Logs
====
Person,Source,Number,Name,When Added,Code
Mia,,2,,2021-03-04,reset()
Mia,,1,None,2021-03-03,go()
Mateo,,1,,2021-03-02,start()
",
                text);
        }

        [Fact]
        public async Task GenerateAsyncValidatesFromDate()
        {
            // Arrange
            var robotType = new RobotType { Id = "types/1", Name = "Nao" };
            using var store = InitialiseDatabase(
                robotType);
            await this.InitialiseIndicesAsync(store, new RobotLogByRobotTypeId());
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<CodePrograms>(session);

            // Act
            generator.UseArguments(new Dictionary<string, string>
            {
                {"from", "Rubbish" },
                {"to", "2021-03-30" },
            });
            var error = await Assert.ThrowsAsync<ApplicationException>(async () => await generator.GenerateAsync(ReportFormat.Text, robotType));

            // Assert
            Assert.Equal("From date is invalid, it should be yyyy-MM-dd, found Rubbish", error.Message);
        }

        [Fact]
        public async Task GenerateAsyncValidatesToDate()
        {
            // Arrange
            var robotType = new RobotType { Id = "types/1", Name = "Nao" };
            using var store = InitialiseDatabase(
                robotType);
            await this.InitialiseIndicesAsync(store, new RobotLogByRobotTypeId());
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<CodePrograms>(session);

            // Act
            generator.UseArguments(new Dictionary<string, string>
            {
                {"from", "2021-03-30" },
                {"to", "Rubbish" },
            });
            var error = await Assert.ThrowsAsync<ApplicationException>(async () => await generator.GenerateAsync(ReportFormat.Text, robotType));

            // Assert
            Assert.Equal("To date is invalid, it should be yyyy-MM-dd, found Rubbish", error.Message);
        }

        [Theory]
        [ReportFormatData(ReportFormat.Excel, ReportFormat.Csv, ReportFormat.Text, ReportFormat.Xml)]
        public void IsFormatAvailableChecksAllowedTypes(ReportFormat format, bool allowed)
        {
            var generator = new CodePrograms();
            Assert.Equal(allowed, generator.IsFormatAvailable(format));
        }
    }
}