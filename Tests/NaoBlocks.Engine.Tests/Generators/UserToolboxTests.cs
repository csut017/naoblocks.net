using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Generators;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Generators
{
    public class UserToolboxTests : DatabaseHelper
    {
        [Fact]
        public async Task GenerateAsyncGeneratesEmptyWithSystemRobot()
        {
            // Arrange
            var user = new User { Id = "users/1", Name = "Mia" };
            var robotType = new RobotType { IsDefault = true };
            using var store = InitialiseDatabase(
                user,
                robotType);
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<UserToolbox>(session);

            // Act
            var output = await generator.GenerateAsync(ReportFormat.Text, user);

            // Assert
            Assert.Equal("Toolbox-Mia.xml", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            Assert.Equal(
                string.Empty,
                text);
        }

        [Fact]
        public async Task GenerateAsyncGeneratesEmptyWithUserRobot()
        {
            // Arrange
            var user = new User { Id = "users/1", Name = "Mia" };
            user.Settings.RobotTypeId = "types/1";
            var robotType = new RobotType { Id = "types/1", IsDefault = true };
            using var store = InitialiseDatabase(
                user,
                robotType);
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<UserToolbox>(session);

            // Act
            var output = await generator.GenerateAsync(ReportFormat.Text, user);

            // Assert
            Assert.Equal("Toolbox-Mia.xml", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            Assert.Equal(
                string.Empty,
                text);
        }

        [Fact]
        public async Task GenerateAsyncThrowsErrorIfInvalidRobotAllocated()
        {
            // Arrange
            var user = new User { Id = "users/1", Name = "Mia" };
            user.Settings.RobotTypeId = "types/1";
            using var store = InitialiseDatabase(
                user);
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<UserToolbox>(session);

            // Act
            var error = await Assert.ThrowsAsync<ApplicationException>(async () => await generator.GenerateAsync(ReportFormat.Text, user));

            // Assert
            Assert.Equal("Unknown robot type", error.Message);
        }

        [Fact]
        public async Task GenerateAsyncThrowsErrorIfNoDefaultRobot()
        {
            // Arrange
            var user = new User { Id = "users/1", Name = "Mia" };
            using var store = InitialiseDatabase(
                user);
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<UserToolbox>(session);

            // Act
            var error = await Assert.ThrowsAsync<ApplicationException>(async () => await generator.GenerateAsync(ReportFormat.Text, user));

            // Assert
            Assert.Equal("Cannot determine robot type", error.Message);
        }

        [Theory]
        [InlineData(ReportFormat.Unknown, false)]
        [InlineData(ReportFormat.Zip, false)]
        [InlineData(ReportFormat.Pdf, false)]
        [InlineData(ReportFormat.Excel, false)]
        [InlineData(ReportFormat.Text, true)]
        [InlineData(ReportFormat.Csv, false)]
        public void IsFormatAvailableChecksAllowedTypes(ReportFormat format, bool allowed)
        {
            var generator = new UserToolbox();
            Assert.Equal(allowed, generator.IsFormatAvailable(format));
        }
    }
}