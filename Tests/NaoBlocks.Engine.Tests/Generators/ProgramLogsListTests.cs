using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Generators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Generators
{
    public class ProgramLogsListTests : DatabaseHelper
    {
        [Fact]
        public async Task GenerateAsyncGeneratesReport()
        {
            // Arrange
            var student = new User { Name = "Mia", Id = "users/1" };
            var robot = new Robot { MachineName = "karetao", FriendlyName = "Mihīni", Id = "robots/1" };
            var conversation = new Conversation { ConversationId = 1124, SourceId = student.Id };
            var timeOff = 0;
            var now = DateTime.Now.AddDays(-1).ToUniversalTime();
            (var log1, timeOff) = this.GenerateLog(conversation, robot, timeOff, now, "tahi", "rua", "toru");
            (var log2, _) = this.GenerateLog(conversation, robot, timeOff, now.AddMinutes(1), "wha", "rima");
            log1.Lines[0].Values.Add(new NamedValue { Name = "info", Value = "ono" });
            log1.Lines[0].Values.Add(new NamedValue { Name = "debug", Value = "whitu" });
            log1.Lines[1].Values.Add(new NamedValue { Name = "error", Value = "waru" });
            log1.Lines[1].Values.Add(new NamedValue { Name = "info", Value = "iwa" });
            log2.Lines[0].Values.Add(new NamedValue { Name = "info", Value = "tekau" });
            log2.Lines[0].Values.Add(new NamedValue { Name = "warn", Value = "tekau mā tahi" });
            using var store = InitialiseDatabase(
                student,
                robot,
                conversation,
                log1,
                log2,
                new CodeProgram { UserId = "Mia", WhenAdded = now, Code = "go()", Number = 1 },
                new CodeProgram { UserId = "Mia", WhenAdded = now.AddMinutes(1), Code = "go()", Number = 2, Name = "hōtaka" });
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<ProgramLogsList>(session);

            // Act
            var output = await generator.GenerateAsync(ReportFormat.Text, student);

            // Assert
            Assert.Equal("Student-Logs-Mia.txt", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            var dateText = now.ToString("yyyy-MM-dd");
            var timeText1 = now.ToString("HH:mm:ss");
            var timeText2 = now.AddMinutes(1).ToString("hh:mm:ss");
            var expected = $@"Logs
====
Robot,Date,Conversation,Time,Type,Description,info,debug,error,warn
Mihīni,{dateText},1124,{timeText1},RobotDebugMessage,tahi,ono,whitu,,
Mihīni,{dateText},1124,{timeText1},RobotDebugMessage,rua,iwa,,waru,
Mihīni,{dateText},1124,{timeText1},RobotDebugMessage,toru,,,,
Mihīni,{dateText},1124,{timeText2},RobotDebugMessage,wha,tekau,,,tekau mā tahi
Mihīni,{dateText},1124,{timeText2},RobotDebugMessage,rima,,,,
Programs
========
Number,When Added,Name,Program
1,{dateText},,go()
2,{dateText},hōtaka,go()
";
            Assert.Equal(expected, text);
        }

        [Fact]
        public async Task GenerateAsyncGeneratesReportWithDateFilter()
        {
            // Arrange
            var student = new User { Name = "Mia", Id = "users/1" };
            var robot = new Robot { MachineName = "karetao", FriendlyName = "Mihīni", Id = "robots/1" };
            var conversation = new Conversation { ConversationId = 1124, SourceId = student.Id };
            var timeOff = 0;
            var now = DateTime.Now.AddDays(-1);
            (var log1, timeOff) = this.GenerateLog(conversation, robot, timeOff, now, "tahi", "rua", "toru");
            (var log2, _) = this.GenerateLog(conversation, robot, timeOff, DefaultTestDateTime, "wha", "rima");
            log1.Lines[0].Values.Add(new NamedValue { Name = "info", Value = "ono" });
            log1.Lines[0].Values.Add(new NamedValue { Name = "debug", Value = "whitu" });
            log1.Lines[1].Values.Add(new NamedValue { Name = "error", Value = "waru" });
            log1.Lines[1].Values.Add(new NamedValue { Name = "info", Value = "iwa" });
            log2.Lines[0].Values.Add(new NamedValue { Name = "info", Value = "tekau" });
            log2.Lines[0].Values.Add(new NamedValue { Name = "warn", Value = "tekau mā tahi" });
            using var store = InitialiseDatabase(
                student,
                robot,
                conversation,
                log1,
                log2,
                new CodeProgram { UserId = "Mia", WhenAdded = now, Code = "go()", Number = 1 },
                new CodeProgram { UserId = "Mia", WhenAdded = DefaultTestDateTime, Code = "go()", Number = 2, Name = "hōtaka" });
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<ProgramLogsList>(session);

            // Act
            generator.UseArguments(new Dictionary<string, string>
            {
                {"from", "2021-03-01" },
                {"to", "2021-03-30" },
            });
            var output = await generator.GenerateAsync(ReportFormat.Text, student);

            // Assert
            Assert.Equal("Student-Logs-Mia.txt", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            var expected = @"Logs
====
Robot,Date,Conversation,Time,Type,Description,info,warn
Mihīni,2021-03-04,1124,05:16:27,RobotDebugMessage,wha,tekau,tekau mā tahi
Mihīni,2021-03-04,1124,05:16:27,RobotDebugMessage,rima,,
Programs
========
Number,When Added,Name,Program
2,2021-03-04,hōtaka,go()
";
            Assert.Equal(expected, text);
        }

        [Theory]
        [InlineData(ReportFormat.Text, "Student-Logs-Mia.txt")]
        [InlineData(ReportFormat.Excel, "Student-Logs-Mia.xlsx")]
        public async Task GenerateAsyncHandlesDifferentFormats(ReportFormat format, string expectedName)
        {
            var student = new User { Name = "Mia", Id = "users/1" };
            var robot = new Robot { MachineName = "karetao", FriendlyName = "Mihīni", Id = "robots/1" };
            var conversation = new Conversation { ConversationId = 1124, SourceId = student.Id };
            using var store = InitialiseDatabase(
                student,
                robot,
                conversation,
                this.GenerateLog(conversation, robot, 0, "tahi", "rua", "toru"),
                this.GenerateLog(conversation, robot, 1, "wha", "rima"));
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<ProgramLogsList>(session);

            // Act
            var output = await generator.GenerateAsync(format, student);

            // Assert
            Assert.Equal(expectedName, output.Item2);
            Assert.True(output.Item1.Length > 0);
        }

        [Theory]
        [ReportFormatData(ReportFormat.Excel, ReportFormat.Pdf, ReportFormat.Text)]
        public void IsFormatAvailableChecksAllowedTypes(ReportFormat format, bool allowed)
        {
            var generator = new ProgramLogsList();
            Assert.Equal(allowed, generator.IsFormatAvailable(format));
        }
    }
}