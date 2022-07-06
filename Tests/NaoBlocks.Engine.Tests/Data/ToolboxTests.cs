using NaoBlocks.Engine.Data;
using Xunit;

namespace NaoBlocks.Engine.Tests.Data
{
    public class ToolboxTests
    {
        [Fact]
        public void ToXmlGeneratesAnXmlStringForAnEmptyToolbox()
        {
            // Arrange
            var toolbox = new Toolbox();

            // Act
            var xml = toolbox.ExportToXml();

            // Assert
            Assert.Equal(
                "<toolbox />",
                xml);
        }

        [Fact]
        public void ToXmlGeneratesAnXmlStringForAToolboxWithMultipleItems()
        {
            // Arrange
            var toolbox = new Toolbox();
            toolbox.Categories.Add(GenerateCategory("first", true, "red",
                GenerateBlock("tahi"),
                GenerateBlock("rua")));
            toolbox.Categories.Add(GenerateCategory("second", false, "blue",
                GenerateBlock("toru")));
            var category = GenerateCategory("third", true, "orange");
            category.Custom = "custom";
            toolbox.Categories.Add(category);

            // Act
            var xml = toolbox.ExportToXml();

            // Assert
            Assert.Equal(
                "<toolbox>" +
                    "<category name=\"first\" colour=\"red\" optional=\"yes\"><block name=\"tahi\"></block><block name=\"rua\"></block></category>" +
                    "<category name=\"second\" colour=\"blue\" optional=\"no\"><block name=\"toru\"></block></category>" +
                    "<category name=\"third\" colour=\"orange\" optional=\"yes\" custom=\"custom\" />" +
                "</toolbox>",
                xml);
        }

        [Fact]
        public void ToXmlGeneratesAnXmlStringForAToolboxWithOneCategory()
        {
            // Arrange
            var toolbox = new Toolbox();
            toolbox.Categories.Add(GenerateCategory("testing", true, "red",
                GenerateBlock("tahi")));

            // Act
            var xml = toolbox.ExportToXml();

            // Assert
            Assert.Equal(
                "<toolbox><category name=\"testing\" colour=\"red\" optional=\"yes\"><block name=\"tahi\"></block></category></toolbox>",
                xml);
        }

        private static ToolboxBlock GenerateBlock(string name)
        {
            return new ToolboxBlock
            {
                Name = name,
                Definition = $"<block name=\"{name}\"></block>"
            };
        }

        private static ToolboxCategory GenerateCategory(string name, bool optional, string colour, params ToolboxBlock[] blocks)
        {
            var category = new ToolboxCategory
            {
                Colour = colour,
                IsOptional = optional,
                Name = name
            };
            foreach (var block in blocks)
            {
                category.Blocks.Add(block);
            }

            return category;
        }
    }
}