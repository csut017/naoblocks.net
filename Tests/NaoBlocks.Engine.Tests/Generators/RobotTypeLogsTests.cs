using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Generators;
using NaoBlocks.Engine.Indices;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Generators
{
    public class RobotTypeLogsTests
        : DatabaseHelper
    {
        [Fact]
        public async Task GenerateAsyncGeneratesReport()
        {
            // Arrange
            var student = new User { Name = "Mia", Id = "users/1" };
            var robotType = new RobotType { Id = "types/1", Name = "Nao" };
            var robot = new Robot { MachineName = "karetao", FriendlyName = "Mihīni", Id = "robots/1", RobotTypeId = robotType.Id };
            var conversation = new Conversation { ConversationId = 1124, SourceId = robot.Id };
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
            await this.InitialiseIndicesAsync(store, new RobotLogByRobotTypeId());
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<RobotTypeLogs>(session);

            // Act
            var output = await generator.GenerateAsync(ReportFormat.Text, robotType);

            // Assert
            Assert.Equal("Nao-logs.txt", output.Item2);
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
        [ReportFormatData(ReportFormat.Excel, ReportFormat.Csv, ReportFormat.Text, ReportFormat.Xml)]
        public void IsFormatAvailableChecksAllowedTypes(ReportFormat format, bool allowed)
        {
            var generator = new RobotTypeLogs();
            Assert.Equal(allowed, generator.IsFormatAvailable(format));
        }
    }
}