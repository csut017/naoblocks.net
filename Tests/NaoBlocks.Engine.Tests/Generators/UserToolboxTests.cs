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
        public async Task GenerateAsyncGeneratesCustomWithSystemRobot()
        {
            // Arrange
            var user = new User { Id = "users/1", Name = "Mia" };
            user.Settings.CustomBlockSet = "simple,sensors";
            var robotType = new RobotType { IsDefault = true };
            robotType.Toolbox.Add(
                GenerateCategory("Simple", "simple",
                    GenerateBlock("tahi", "<rua/>", 3)));
            robotType.Toolbox.Add(
                GenerateCategory("Default", "default",
                    GenerateBlock("whitu", "<waru/>", 9),
                    GenerateBlock("wha", "<rima/>", 6)));
            var sensors = GenerateCategory("Sensors", "sensors",
                    GenerateBlock("camera", "<definition/>", 1));
            sensors.Custom = "custom";
            robotType.Toolbox.Add(sensors);
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
                @"<?xml version=""1.0"" encoding=""utf-8""?><xml><category name=""Sensors"" colour=""1"" custom=""custom""><definition /></category><category name=""Simple"" colour=""1""><rua /></category></xml>",
                text);
        }

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
        public async Task GenerateAsyncGeneratesSimpleWithSystemRobot()
        {
            // Arrange
            var user = new User { Id = "users/1", Name = "Mia" };
            user.Settings.Simple = true;
            var robotType = new RobotType { IsDefault = true };
            robotType.Toolbox.Add(
                GenerateCategory("Simple", "simple",
                    GenerateBlock("tahi", "<rua/>", 3)));
            robotType.Toolbox.Add(
                GenerateCategory("Default", "default",
                    GenerateBlock("whitu", "<waru/>", 9),
                    GenerateBlock("wha", "<rima/>", 6)));
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
                @"<?xml version=""1.0"" encoding=""utf-8""?><xml><category name=""Simple"" colour=""1""><rua /></category></xml>",
                text);
        }

        [Fact]
        public async Task GenerateAsyncGeneratesWithAllOptions()
        {
            // Arrange
            var user = new User { Id = "users/1", Name = "Mia" };
            user.Settings.Conditionals = true;
            user.Settings.Dances = true;
            user.Settings.Events = true;
            user.Settings.Loops = true;
            user.Settings.Sensors = true;
            user.Settings.Variables = true;
            var robotType = new RobotType { IsDefault = true };
            robotType.Toolbox.Add(GenerateCategory("Simple", "simple"));
            robotType.Toolbox.Add(GenerateCategory("Default", "default"));
            robotType.Toolbox.Add(GenerateCategory("Conditionals", "conditionals"));
            robotType.Toolbox.Add(GenerateCategory("Dances", "dances"));
            robotType.Toolbox.Add(GenerateCategory("Events", "events"));
            robotType.Toolbox.Add(GenerateCategory("Loops", "loops"));
            robotType.Toolbox.Add(GenerateCategory("Sensors", "sensors"));
            robotType.Toolbox.Add(GenerateCategory("Variables", "variables"));
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
                @"<?xml version=""1.0"" encoding=""utf-8""?><xml><category name=""Conditionals"" colour=""1"" /><category name=""Dances"" colour=""1"" /><category name=""Default"" colour=""1"" /><category name=""Events"" colour=""1"" /><category name=""Loops"" colour=""1"" /><category name=""Sensors"" colour=""1"" /><category name=""Variables"" colour=""1"" /></xml>",
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

        private static ToolboxBlock GenerateBlock(string name, string definition, int order)
        {
            return new ToolboxBlock
            {
                Name = name,
                Definition = definition,
                Order = order
            };
        }

        private static ToolboxCategory GenerateCategory(string name, string tags, params ToolboxBlock[] blocks)
        {
            var category = new ToolboxCategory
            {
                Name = name,
                Colour = "1",
                Order = 2
            };
            foreach (var tag in tags.Split(","))
            {
                category.Tags.Add(tag);
            }
            foreach (var block in blocks)
            {
                category.Blocks.Add(block);
            }
            return category;
        }
    }
}