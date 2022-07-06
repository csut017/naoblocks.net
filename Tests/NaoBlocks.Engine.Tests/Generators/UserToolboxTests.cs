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
        public async Task GenerateAsyncHandlesOptional()
        {
            // Arrange
            var user = new User { Id = "users/1", Name = "Mia" };
            user.Settings.RobotTypeId = "types/1";
            user.Settings.Toolbox = "rua";
            user.Settings.ToolboxCategories.Add("kowhai");
            var robotType = new RobotType { Id = "types/1", IsDefault = true };
            robotType.Toolboxes.Add(
                GenerateToolbox(
                    "tahi",
                    true,
                    GenerateCategory("whero", false, new ToolboxBlock { Name = "block1", Definition = "<block/>" }),
                    GenerateCategory("kowhai", true, new ToolboxBlock { Name = "block1", Definition = "<block/>" }),
                    GenerateCategory("kakriki", true, new ToolboxBlock { Name = "block1", Definition = "<block/>" })));
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
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><xml><category name=\"whero\" colour=\"0\"><block /></category><category name=\"kowhai\" colour=\"0\"><block /></category></xml>",
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

        [Fact]
        public async Task GenerateAsyncUsesUserRequestedToolbox()
        {
            // Arrange
            var user = new User { Id = "users/1", Name = "Mia" };
            user.Settings.RobotTypeId = "types/1";
            user.Settings.Toolbox = "rua";
            var robotType = new RobotType { Id = "types/1", IsDefault = true };
            robotType.Toolboxes.Add(
                GenerateToolbox(
                    "tahi",
                    true,
                    GenerateCategory("whero", new ToolboxBlock { Name = "block1", Definition = "<block/>" })));
            robotType.Toolboxes.Add(
                GenerateToolbox(
                    "rua",
                    false,
                    GenerateCategory("kakariki", new ToolboxBlock { Name = "block2", Definition = "<block/>" })));
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
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><xml><category name=\"kakariki\" colour=\"0\"><block /></category></xml>",
                text);
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

        private static ToolboxCategory GenerateCategory(string name, params ToolboxBlock[] blocks)
        {
            var category = new ToolboxCategory
            {
                Name = name,
            };
            foreach (var block in blocks)
            {
                category.Blocks.Add(block);
            }
            return category;
        }

        private static ToolboxCategory GenerateCategory(string name, bool isOptional, params ToolboxBlock[] blocks)
        {
            var category = new ToolboxCategory
            {
                Name = name,
                IsOptional = isOptional,
            };
            foreach (var block in blocks)
            {
                category.Blocks.Add(block);
            }
            return category;
        }

        private static Toolbox GenerateToolbox(string name, bool isDefault, params ToolboxCategory[] categories)
        {
            var toolbox = new Toolbox
            {
                Name = name,
                IsDefault = isDefault
            };
            foreach (var category in categories)
            {
                toolbox.Categories.Add(category);
            }
            return toolbox;
        }
    }
}