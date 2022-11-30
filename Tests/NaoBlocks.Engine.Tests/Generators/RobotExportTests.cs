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
    public class RobotExportTests
        : DatabaseHelper
    {
        [Fact]
        public async Task GenerateAsyncGeneratesReport()
        {
            // Arrange
            var user = new User { Id = "users/1", Name = "Mia" };
            var robotType = new RobotType { Id = "types/1", Name = "Nao" };
            var robot = new Robot { MachineName = "karetao", FriendlyName = "Mihīni", Id = "robots/1", RobotTypeId = robotType.Id, IsInitialised = true };
            var conversation = new Conversation { ConversationId = 1124, SourceId = user.Id, SourceType = "User", ConversationType = ConversationType.Program };
            var timeOff = 0;
            var logTime = DateTime.UtcNow.AddDays(-1);
            (var log1, timeOff) = this.GenerateLog(conversation, robot, timeOff, logTime, "tahi", "rua", "toru");
            (var log2, _) = this.GenerateLog(conversation, robot, timeOff, logTime, "wha", "rima");
            log1.Lines[0].Values.Add(new NamedValue { Name = "info", Value = "ono" });
            log1.Lines[0].Values.Add(new NamedValue { Name = "debug", Value = "whitu" });
            log1.Lines[1].Values.Add(new NamedValue { Name = "error", Value = "waru" });
            log1.Lines[1].Values.Add(new NamedValue { Name = "info", Value = "iwa" });
            log2.Lines[0].Values.Add(new NamedValue { Name = "info", Value = "tekau" });
            log2.Lines[0].Values.Add(new NamedValue { Name = "warn", Value = "tekau mā tahi" });
            using var store = InitialiseDatabase(
                robot,
                conversation,
                log1,
                log2);
            await this.InitialiseIndicesAsync(store, new RobotLogByRobotTypeId());
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<RobotExport>(session);

            // Act
            var output = await generator.GenerateAsync(ReportFormat.Text, robot);

            // Assert
            Assert.Equal("Robot-Export-Mihīni.txt", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            var formattedDate = logTime.ToLocalTime().ToString("yyyy-MM-dd");
            var formattedTime = logTime.ToLocalTime().ToString("HH:mm:ss");
            Assert.Equal(
                    @$"Details
=======
Machine Name: karetao
Friendly Name: Mihīni
Type: <Unknown>
Logs
====
Robot,Date,Conversation,Time,Type,Description,info,debug,error,warn
Mihīni,{formattedDate},1124,{formattedTime},RobotDebugMessage,tahi,ono,whitu,,
Mihīni,{formattedDate},1124,{formattedTime},RobotDebugMessage,rua,iwa,,waru,
Mihīni,{formattedDate},1124,{formattedTime},RobotDebugMessage,toru,,,,
Mihīni,{formattedDate},1124,{formattedTime},RobotDebugMessage,wha,tekau,,,tekau mā tahi
Mihīni,{formattedDate},1124,{formattedTime},RobotDebugMessage,rima,,,,
",
                text);
        }

        [Fact]
        public async Task GenerateAsyncGeneratesReportWithDateRange()
        {
            // Arrange
            var user = new User { Id = "users/1", Name = "Mia" };
            var robotType = new RobotType { Id = "types/1", Name = "Nao" };
            var robot = new Robot { MachineName = "karetao", FriendlyName = "Mihīni", Id = "robots/1", RobotTypeId = robotType.Id, IsInitialised = true, Type = robotType };
            var conversation = new Conversation { ConversationId = 1124, SourceId = user.Id, SourceType = "User", ConversationType = ConversationType.Program };
            var timeOff = 0;
            (var log1, timeOff) = this.GenerateLog(conversation, robot, timeOff, "tahi", "rua", "toru");
            (var log2, _) = this.GenerateLog(conversation, robot, timeOff, DateTime.Now.AddDays(-1), "wha", "rima");
            log1.Lines[0].Values.Add(new NamedValue { Name = "info", Value = "ono" });
            log1.Lines[0].Values.Add(new NamedValue { Name = "debug", Value = "whitu" });
            log1.Lines[1].Values.Add(new NamedValue { Name = "error", Value = "waru" });
            log1.Lines[1].Values.Add(new NamedValue { Name = "token", Value = "This should be ignored" });
            log1.Lines[1].Values.Add(new NamedValue { Name = "info", Value = "iwa" });
            log2.Lines[0].Values.Add(new NamedValue { Name = "info", Value = "tekau" });
            log2.Lines[0].Values.Add(new NamedValue { Name = "warn", Value = "tekau mā tahi" });
            using var store = InitialiseDatabase(
                robot,
                conversation,
                log1,
                log2);
            await this.InitialiseIndicesAsync(store, new RobotLogByRobotTypeId());
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<RobotExport>(session);

            // Act
            generator.UseArguments(new Dictionary<string, string>
            {
                {"from", "2021-03-01" },
                {"to", "2021-03-30" },
            });
            var output = await generator.GenerateAsync(ReportFormat.Text, robot);

            // Assert
            Assert.Equal("Robot-Export-Mihīni.txt", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            Assert.Equal(
                    @"Details
=======
Machine Name: karetao
Friendly Name: Mihīni
Type: Nao
Logs
====
Robot,Date,Conversation,Time,Type,Description,info,debug,error
Mihīni,2021-03-04,1124,18:16:27,RobotDebugMessage,tahi,ono,whitu,
Mihīni,2021-03-04,1124,18:16:27,RobotDebugMessage,rua,iwa,,waru
Mihīni,2021-03-04,1124,18:16:27,RobotDebugMessage,toru,,,
",
                text);
        }

        [Fact]
        public async Task GenerateAsyncGeneratesReportWithoutLogs()
        {
            // Arrange
            var user = new User { Id = "users/1", Name = "Mia" };
            var robotType = new RobotType { Id = "types/1", Name = "Nao" };
            var robot = new Robot { MachineName = "karetao", FriendlyName = "Mihīni", Id = "robots/1", RobotTypeId = robotType.Id, IsInitialised = true };
            var conversation = new Conversation { ConversationId = 1124, SourceId = user.Id, SourceType = "User", ConversationType = ConversationType.Program };
            var timeOff = 0;
            var logTime = DateTime.UtcNow.AddDays(-1);
            (var log1, timeOff) = this.GenerateLog(conversation, robot, timeOff, logTime, "tahi", "rua", "toru");
            (var log2, _) = this.GenerateLog(conversation, robot, timeOff, logTime, "wha", "rima");
            log1.Lines[0].Values.Add(new NamedValue { Name = "info", Value = "ono" });
            log1.Lines[0].Values.Add(new NamedValue { Name = "debug", Value = "whitu" });
            log1.Lines[1].Values.Add(new NamedValue { Name = "error", Value = "waru" });
            log1.Lines[1].Values.Add(new NamedValue { Name = "info", Value = "iwa" });
            log2.Lines[0].Values.Add(new NamedValue { Name = "info", Value = "tekau" });
            log2.Lines[0].Values.Add(new NamedValue { Name = "warn", Value = "tekau mā tahi" });
            using var store = InitialiseDatabase(
                robot,
                robotType,
                conversation,
                log1,
                log2);
            await this.InitialiseIndicesAsync(store, new RobotLogByRobotTypeId());
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<RobotExport>(session);

            // Act
            generator.UseArguments(new Dictionary<string, string>
            {
                { "logs","no" }
            });
            var output = await generator.GenerateAsync(ReportFormat.Text, robot);

            // Assert
            Assert.Equal("Robot-Export-Mihīni.txt", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            Assert.Equal(
                    @$"Details
=======
Machine Name: karetao
Friendly Name: Mihīni
Type: Nao
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
            var generator = InitialiseGenerator<RobotExport>(session);

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
            var generator = InitialiseGenerator<RobotExport>(session);

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
        [ReportFormatData(ReportFormat.Excel, ReportFormat.Csv, ReportFormat.Text, ReportFormat.Xml, ReportFormat.Pdf)]
        public void IsFormatAvailableChecksAllowedTypes(ReportFormat format, bool allowed)
        {
            var generator = new RobotExport();
            Assert.Equal(allowed, generator.IsFormatAvailable(format));
        }
    }
}