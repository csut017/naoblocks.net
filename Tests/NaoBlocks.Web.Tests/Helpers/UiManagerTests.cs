using NaoBlocks.Web.Helpers;
using System;
using Xunit;

namespace NaoBlocks.Web.Tests.Helpers
{
    public class UiManagerTests
    {
        [Fact]
        public void ParseConvertsData()
        {
            // Arrange
            var manager = new UiManager();
            manager.Register<FakeUIDefinition>("angular", () => "default");

            // Act
            var data = manager.Parse("angular", @"{""data"":""check""}");

            // Assert
            var definition = Assert.IsType<FakeUIDefinition>(data);
            Assert.Equal("check", definition?.Data);
        }

        [Fact]
        public void ParseConvertsEmptyData()
        {
            // Arrange
            var manager = new UiManager();
            manager.Register<FakeUIDefinition>("angular", () => "default");

            // Act
            var data = manager.Parse("angular", "{}");

            // Assert
            var definition = Assert.IsType<FakeUIDefinition>(data);
            Assert.Equal(String.Empty, definition?.Data);
        }

        [Fact]
        public void ParseHandesBadData()
        {
            // Arrange
            var manager = new UiManager();
            manager.Register<FakeUIDefinition>("angular", () => "default");

            // Act
            var error = Assert.Throws<ApplicationException>(() => manager.Parse("angular", "bad"));

            // Assert
            Assert.Equal("Unable to parse definition", error.Message);
        }

        [Fact]
        public void ParseHandlesMissingDefinition()
        {
            // Arrange
            var manager = new UiManager();

            // Act
            var error = Assert.Throws<ApplicationException>(() => manager.Parse("angular", "{}"));

            // Assert
            Assert.Equal("UI angular does not exist", error.Message);
        }
    }
}