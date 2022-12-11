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
    public class RobotLogsTests
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
            (var log1, timeOff) = this.GenerateLog(conversation, robot, timeOff, DateTime.Now.AddDays(-1), "tahi", "rua", "toru");
            (var log2, _) = this.GenerateLog(conversation, robot, timeOff, DateTime.Now.AddDays(-1), "wha", "rima");
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
            var generator = InitialiseGenerator<RobotLogs>(session);

            // Act
            var output = await generator.GenerateAsync(ReportFormat.Text, robotType);

            // Assert
            Assert.Equal("Nao-logs.txt", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            Assert.Matches(
                    @"Logs
====
Robot,Date,Conversation,Time,Type,Description,info,warn,debug,error
Mihīni,\d{4}-\d{2}-\d{2},1124,\d{2}:\d{2}:\d{2},RobotDebugMessage,wha,tekau,tekau mā tahi,,
Mihīni,\d{4}-\d{2}-\d{2},1124,\d{2}:\d{2}:\d{2},RobotDebugMessage,rima,,,,
Mihīni,\d{4}-\d{2}-\d{2},1124,\d{2}:\d{2}:\d{2},RobotDebugMessage,tahi,ono,,whitu,
Mihīni,\d{4}-\d{2}-\d{2},1124,\d{2}:\d{2}:\d{2},RobotDebugMessage,rua,iwa,,,waru
Mihīni,\d{4}-\d{2}-\d{2},1124,\d{2}:\d{2}:\d{2},RobotDebugMessage,toru,,,,
",
                text);
        }

        [Fact]
        public async Task GenerateAsyncGeneratesReportIgnoresTokens()
        {
            // Arrange
            var user = new User { Id = "users/1", Name = "Mia" };
            var robotType = new RobotType { Id = "types/1", Name = "Nao" };
            var robot = new Robot { MachineName = "karetao", FriendlyName = "Mihīni", Id = "robots/1", RobotTypeId = robotType.Id, IsInitialised = true };
            var conversation = new Conversation { ConversationId = 1124, SourceId = user.Id, SourceType = "User", ConversationType = ConversationType.Program };
            var timeOff = 0;
            (var log1, timeOff) = this.GenerateLog(conversation, robot, timeOff, "tahi", "rua", "toru");
            log1.Lines[0].Values.Add(new NamedValue { Name = "info", Value = "ono" });
            log1.Lines[0].Values.Add(new NamedValue { Name = "debug", Value = "whitu" });
            log1.Lines[1].Values.Add(new NamedValue { Name = "error", Value = "waru" });
            log1.Lines[1].Values.Add(new NamedValue { Name = "token", Value = "This should be ignored" });
            log1.Lines[1].Values.Add(new NamedValue { Name = "info", Value = "iwa" });
            using var store = InitialiseDatabase(
                robot,
                conversation,
                log1);
            await this.InitialiseIndicesAsync(store, new RobotLogByRobotTypeId());
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<RobotLogs>(session);

            // Act
            generator.UseArguments(new Dictionary<string, string>
            {
                {"from", "2021-03-01" },
                {"to", "2021-03-30" },
            });
            var output = await generator.GenerateAsync(ReportFormat.Text, robotType);

            // Assert
            Assert.Equal("Nao-logs.txt", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            Assert.Equal(
                    @"Logs
====
Robot,Date,Conversation,Time,Type,Description,info,debug,error
Mihīni,2021-03-04,1124,18:16:27,RobotDebugMessage,tahi,ono,whitu,
Mihīni,2021-03-04,1124,18:16:27,RobotDebugMessage,rua,iwa,,waru
Mihīni,2021-03-04,1124,18:16:27,RobotDebugMessage,toru,,,
",
                text);
        }

        [Fact]
        public async Task GenerateAsyncGeneratesReportWithDateRange()
        {
            // Arrange
            var user = new User { Id = "users/1", Name = "Mia" };
            var robotType = new RobotType { Id = "types/1", Name = "Nao" };
            var robot = new Robot { MachineName = "karetao", FriendlyName = "Mihīni", Id = "robots/1", RobotTypeId = robotType.Id, IsInitialised = true };
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
            var generator = InitialiseGenerator<RobotLogs>(session);

            // Act
            generator.UseArguments(new Dictionary<string, string>
            {
                {"from", "2021-03-01" },
                {"to", "2021-03-30" },
            });
            var output = await generator.GenerateAsync(ReportFormat.Text, robotType);

            // Assert
            Assert.Equal("Nao-logs.txt", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            Assert.Equal(
                    @"Logs
====
Robot,Date,Conversation,Time,Type,Description,info,debug,error
Mihīni,2021-03-04,1124,18:16:27,RobotDebugMessage,tahi,ono,whitu,
Mihīni,2021-03-04,1124,18:16:27,RobotDebugMessage,rua,iwa,,waru
Mihīni,2021-03-04,1124,18:16:27,RobotDebugMessage,toru,,,
",
                text);
        }

        [Fact]
        public async Task GenerateAsyncGeneratesSystemReport()
        {
            // Arrange
            var user = new User { Id = "users/1", Name = "Mia" };
            var robotType1 = new RobotType { Id = "types/1", Name = "Nao" };
            var robotType2 = new RobotType { Id = "types/2", Name = "Nao" };
            var robot1 = new Robot { MachineName = "karetao", FriendlyName = "Mihīni Tahi", Id = "robots/1", RobotTypeId = robotType1.Id, IsInitialised = true };
            var robot2 = new Robot { MachineName = "karetao", FriendlyName = "Mihīni Rua", Id = "robots/2", RobotTypeId = robotType2.Id, IsInitialised = true };
            var conversation = new Conversation { ConversationId = 1124, SourceId = user.Id, SourceType = "User", ConversationType = ConversationType.Program };
            var timeOff = 0;
            (var log1, timeOff) = this.GenerateLog(conversation, robot1, timeOff, "tahi", "rua", "toru");
            (var log2, _) = this.GenerateLog(conversation, robot2, timeOff, "wha", "rima");
            log1.Lines[0].Values.Add(new NamedValue { Name = "info", Value = "ono" });
            log1.Lines[0].Values.Add(new NamedValue { Name = "debug", Value = "whitu" });
            log1.Lines[1].Values.Add(new NamedValue { Name = "error", Value = "waru" });
            log1.Lines[1].Values.Add(new NamedValue { Name = "token", Value = "This should be ignored" });
            log1.Lines[1].Values.Add(new NamedValue { Name = "info", Value = "iwa" });
            log2.Lines[0].Values.Add(new NamedValue { Name = "info", Value = "tekau" });
            log2.Lines[0].Values.Add(new NamedValue { Name = "warn", Value = "tekau mā tahi" });
            using var store = InitialiseDatabase(
                robot1,
                robot2,
                conversation,
                log1,
                log2,
                robotType1);
            await this.InitialiseIndicesAsync(store, new RobotLogByRobotTypeId());
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<RobotLogs>(session);

            // Act
            generator.UseArguments(new Dictionary<string, string>
            {
                {"from", "2021-03-01" },
                {"to", "2021-03-30" },
            });
            var output = await generator.GenerateAsync(ReportFormat.Text);

            // Assert
            Assert.Equal("All-logs.txt", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            Assert.Equal(
                    @"Logs
====
Robot Type,Robot,Date,Conversation,Time,Type,Description,info,debug,error,warn
Nao,Mihīni Tahi,2021-03-04,1124,18:16:27,RobotDebugMessage,tahi,ono,whitu,,
Nao,Mihīni Tahi,2021-03-04,1124,18:16:27,RobotDebugMessage,rua,iwa,,waru,
Nao,Mihīni Tahi,2021-03-04,1124,18:16:27,RobotDebugMessage,toru,,,,
<Unknown>,Mihīni Rua,2021-03-04,1124,18:16:27,RobotDebugMessage,wha,tekau,,,tekau mā tahi
<Unknown>,Mihīni Rua,2021-03-04,1124,18:16:27,RobotDebugMessage,rima,,,,
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
            var generator = InitialiseGenerator<RobotLogs>(session);

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
            var generator = InitialiseGenerator<RobotLogs>(session);

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
            var generator = new RobotLogs();
            Assert.Equal(allowed, generator.IsFormatAvailable(format));
        }
    }
}