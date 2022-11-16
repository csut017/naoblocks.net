using System;
using System.Collections.Generic;
using Xunit;

namespace NaoBlocks.Engine.Tests
{
    public class TemplateGeneratorTests
    {
        [Fact]
        public void ReadEmbededResourceReturnsContent()
        {
            // Act
            var content = TemplateGenerator.ReadEmbededResource<TemplateGeneratorTests>("test.template");

            // Assert
            Assert.Equal("Data: <[[data]]>", content);
        }

        [Fact]
        public void ReadEmbededResourceHandlesInvalidName()
        {
            // Act
            var error = Assert.Throws<ArgumentException>(
                () => TemplateGenerator.ReadEmbededResource<TemplateGeneratorTests>(string.Empty));

            // Assert
            Assert.Equal("name", error.ParamName);
        }

        [Fact]
        public void ReadEmbededResourceHandlesMissingResource()
        {
            // Act
            var error = Assert.Throws<ApplicationException>(
                () => TemplateGenerator.ReadEmbededResource<TemplateGeneratorTests>("missing.template"));

            // Assert
            Assert.Equal("Unable to find resource missing.template", error.Message);
        }

        [Theory]
        [InlineData("<[[missing]]>", "")]
        [InlineData("<[[data]]>", "Replaced")]
        [InlineData("<[[data]]>-<[[data]]>", "Replaced-Replaced")]
        public void BuildGeneratesContent(string template, string expected)
        {
            // Act
            var builders = new Dictionary<string, Func<string>>
            {
                { "data", () => "Replaced" }
            };
            var output = TemplateGenerator.Build(template, builders);

            // Assert
            Assert.Equal(expected, output);
        }

        [Theory]
        [InlineData("test")]
        [InlineData("test.template")]
        public void BuildFromTemplateGeneratesContent(string name)
        {
            // Act
            var builders = new Dictionary<string, Func<string>>
            {
                { "data", () => "Replaced" }
            };
            var content = TemplateGenerator.BuildFromTemplate<TemplateGeneratorTests>(name, builders);

            // Assert
            Assert.Equal("Data: Replaced", content);
        }
    }
}
