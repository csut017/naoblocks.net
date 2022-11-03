using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Generators;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Generators
{
    public class SnapshotsListTests : DatabaseHelper
    {
        [Fact]
        public async Task GenerateAsyncGeneratesReport()
        {
            // Arrange
            var student = new User { Id = "users/1", Name = "Mia" };
            var now = DateTime.Now.AddDays(-1).ToUniversalTime();
            var snapshot1 = new Snapshot
            {
                UserId = student.Id,
                WhenAdded = now,
                Source = "karetao",
                State = "valid"
            };
            snapshot1.Values.Add(new NamedValue { Name = "tahi", Value = "rua" });
            snapshot1.Values.Add(new NamedValue { Name = "toru", Value = "wha" });
            var snapshot2 = new Snapshot
            {
                UserId = student.Id,
                WhenAdded = now.AddMinutes(1),
                Source = "karetao",
                State = "invalid"
            };
            snapshot2.Values.Add(new NamedValue { Name = "tahi", Value = "rima" });
            using var store = InitialiseDatabase(
                student,
                snapshot2,
                snapshot1);
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<UserSnapshots>(session);

            // Act
            var output = await generator.GenerateAsync(ReportFormat.Text, student);

            // Assert
            Assert.Equal("Snapshots-Mia.txt", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            var dateTimeText1 = now.ToString("yyyy-MM-dd HH:mm:ss");
            var dateTimeText2 = now.AddMinutes(1).ToString("yyyy-MM-dd HH:mm:ss");
            var expected = $@"Snapshots
=========
Date/Time,Source,State,tahi,toru
{dateTimeText1},karetao,valid,rua,wha
{dateTimeText2},karetao,invalid,rima,
";
            Assert.Equal(expected, text);
        }

        [Fact]
        public async Task GenerateAsyncGeneratesReportWithDateFilter()
        {
            // Arrange
            var student = new User { Id = "users/1", Name = "Mia" };
            var now = DateTime.Now.AddDays(-1);
            var snapshot1 = new Snapshot
            {
                UserId = student.Id,
                WhenAdded = now,
                Source = "karetao",
                State = "valid"
            };
            snapshot1.Values.Add(new NamedValue { Name = "tahi", Value = "rua" });
            snapshot1.Values.Add(new NamedValue { Name = "toru", Value = "wha" });
            var snapshot2 = new Snapshot
            {
                UserId = student.Id,
                WhenAdded = DefaultTestDateTime.AddMinutes(1),
                Source = "karetao",
                State = "invalid"
            };
            snapshot2.Values.Add(new NamedValue { Name = "tahi", Value = "rima" });
            using var store = InitialiseDatabase(
                student,
                snapshot2,
                snapshot1);
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<UserSnapshots>(session);

            // Act
            var output = await generator.GenerateAsync(ReportFormat.Text, student);

            // Assert
            Assert.Equal("Snapshots-Mia.txt", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            var dateTimeText = now.ToString("yyyy-MM-dd HH:mm:ss");
            var expected = $@"Snapshots
=========
Date/Time,Source,State,tahi,toru
{dateTimeText},karetao,valid,rua,wha
";
            Assert.Equal(expected, text);
        }

        [Theory]
        [InlineData(ReportFormat.Csv, "Snapshots-Mia.csv")]
        [InlineData(ReportFormat.Text, "Snapshots-Mia.txt")]
        [InlineData(ReportFormat.Excel, "Snapshots-Mia.xlsx")]
        public async Task GenerateAsyncHandlesDifferentFormats(ReportFormat format, string expectedName)
        {
            var student = new User { Id = "users/1", Name = "Mia" };
            var snapshot = new Snapshot
            {
                UserId = student.Id,
                WhenAdded = DefaultTestDateTime,
                Source = "karetao",
                State = "valid"
            };
            snapshot.Values.Add(new NamedValue { Name = "tahi", Value = "rua" });
            snapshot.Values.Add(new NamedValue { Name = "tahi", Value = "toru" });
            using var store = InitialiseDatabase(
                student,
                snapshot);
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<UserSnapshots>(session);

            // Act
            var output = await generator.GenerateAsync(format, student);

            // Assert
            Assert.Equal(expectedName, output.Item2);
            Assert.True(output.Item1.Length > 0);
        }

        [Theory]
        [ReportFormatData(ReportFormat.Excel, ReportFormat.Pdf, ReportFormat.Text, ReportFormat.Csv)]
        public void IsFormatAvailableChecksAllowedTypes(ReportFormat format, bool allowed)
        {
            var generator = new UserSnapshots();
            Assert.Equal(allowed, generator.IsFormatAvailable(format));
        }
    }
}