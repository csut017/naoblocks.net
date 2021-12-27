using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Generators;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Generators
{
    public class ProgramLogsListTests : DatabaseHelper
    {
        private readonly DateTime now = new(2021, 3, 4, 5, 16, 27);

        [Fact]
        public async Task GenerateAsyncGeneratesReport()
        {
            // Arrange
            var student = new User { Name = "Mia", Id = "users/1" };
            var robot = new Robot { MachineName = "karetao", FriendlyName = "Mihīni", Id = "robots/1" };
            var conversation = new Conversation { ConversationId = 1124, UserId = student.Id };
            var timeOff = 0;
            (var log1, timeOff) = this.GenerateLog(conversation, robot, timeOff, "tahi", "rua", "toru");
            (var log2, _) = this.GenerateLog(conversation, robot, timeOff, "wha", "rima");
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
                new CodeProgram { UserId = "Mia", WhenAdded = now, Code = "go()", Number = 2, Name = "hōtaka" });
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<ProgramLogsList>(session);

            // Act
            var output = await generator.GenerateAsync(ReportFormat.Text, student);

            // Assert
            Assert.Equal("Student-Logs-Mia.txt", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            Assert.Equal(
                    @"Logs
====
Robot,Date,Conversation,Time,Type,Description,info,debug,error,warn
Mihīni,2021-03-04,1124,05:16:27,RobotDebugMessage,tahi,ono,whitu,,
Mihīni,2021-03-04,1124,05:16:27,RobotDebugMessage,rua,iwa,,waru,
Mihīni,2021-03-04,1124,05:16:27,RobotDebugMessage,toru,,,,
Mihīni,2021-03-04,1124,05:16:27,RobotDebugMessage,wha,tekau,,,tekau mā tahi
Mihīni,2021-03-04,1124,05:16:27,RobotDebugMessage,rima,,,,
Programs
========
Number,When Added,Name,Program
1,2021-03-04,,go()
2,2021-03-04,hōtaka,go()
",
                text);
        }

        [Theory]
        [InlineData(ReportFormat.Text, "Student-Logs-Mia.txt")]
        [InlineData(ReportFormat.Excel, "Student-Logs-Mia.xlsx")]
        public async Task GenerateAsyncHandlesDifferentFormats(ReportFormat format, string expectedName)
        {
            var student = new User { Name = "Mia", Id = "users/1" };
            var robot = new Robot { MachineName = "karetao", FriendlyName = "Mihīni", Id = "robots/1" };
            var conversation = new Conversation { ConversationId = 1124, UserId = student.Id };
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
        [InlineData(ReportFormat.Unknown, false)]
        [InlineData(ReportFormat.Zip, false)]
        [InlineData(ReportFormat.Pdf, true)]
        [InlineData(ReportFormat.Excel, true)]
        [InlineData(ReportFormat.Text, true)]
        [InlineData(ReportFormat.Csv, false)]
        public void IsFormatAvailableChecksAllowedTypes(ReportFormat format, bool allowed)
        {
            var generator = new ProgramLogsList();
            Assert.Equal(allowed, generator.IsFormatAvailable(format));
        }

        private (RobotLog, int) GenerateLog(Conversation conversation, Robot robot, int timeOffset, params string[] messages)
        {
            var log = new RobotLog
            {
                Conversation = conversation,
                RobotId = robot.Id,
                WhenAdded = now.AddMinutes(-1),
                WhenLastUpdated = now
            };

            var count = timeOffset;
            foreach (var message in messages)
            {
                var line = new RobotLogLine
                {
                    Description = message,
                    SourceMessageType = ClientMessageType.RobotDebugMessage,
                    WhenAdded = now.AddMinutes(timeOffset),
                };
                log.Lines.Add(line);
            }
            return (log, timeOffset);
        }
    }
}