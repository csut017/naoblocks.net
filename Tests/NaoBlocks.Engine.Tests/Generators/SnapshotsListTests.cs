using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Generators;
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
            var generator = InitialiseGenerator<SnapshotsList>(session);

            // Act
            var output = await generator.GenerateAsync(ReportFormat.Text, student);

            // Assert
            Assert.Equal("Snapshots-Mia.txt", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            Assert.Equal(
                    @"Snapshots
=========
Date/Time,Source,State,tahi,toru
2021-03-04 05:16:27,karetao,valid,rua,wha
2021-03-04 05:17:27,karetao,invalid,rima,
",
                text);
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
                WhenAdded = now,
                Source = "karetao",
                State = "valid"
            };
            snapshot.Values.Add(new NamedValue { Name = "tahi", Value = "rua" });
            snapshot.Values.Add(new NamedValue { Name = "tahi", Value = "toru" });
            using var store = InitialiseDatabase(
                student,
                snapshot);
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<SnapshotsList>(session);

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
            var generator = new SnapshotsList();
            Assert.Equal(allowed, generator.IsFormatAvailable(format));
        }
    }
}