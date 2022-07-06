using NaoBlocks.Engine.Data;
using System;
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
            var xml = toolbox.ExportToXml(Toolbox.Format.Toolbox);

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
            var xml = toolbox.ExportToXml(Toolbox.Format.Toolbox);

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
        public void ToXmlGeneratesAnXmlStringForAToolboxWithMultipleItemsForBlockly()
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
            var xml = toolbox.ExportToXml(Toolbox.Format.Blockly);

            // Assert
            Assert.Equal(
                "<xml>" +
                    "<block type=\"category\">" +
                        "<field name=\"NAME\">first</field>" +
                        "<field name=\"OPTIONAL\">TRUE</field>" +
                        "<field name=\"COLOUR\">red</field>" +
                        "<statement name=\"BLOCKS\">" +
                            "<block type=\"tahi\">" +
                                "<next>" +
                                    "<block type=\"rua\" />" +
                                "</next>" +
                            "</block>" +
                        "</statement>" +
                        "<next>" +
                            "<block type=\"category\">" +
                                "<field name=\"NAME\">second</field>" +
                                "<field name=\"OPTIONAL\">FALSE</field>" +
                                "<field name=\"COLOUR\">blue</field>" +
                                "<statement name=\"BLOCKS\">" +
                                    "<block type=\"toru\" />" +
                                "</statement>" +
                                    "<next>" +
                                        "<block type=\"category\">" +
                                            "<field name=\"NAME\">third</field>" +
                                            "<field name=\"OPTIONAL\">TRUE</field>" +
                                            "<field name=\"COLOUR\">orange</field>" +
                                            "<statement name=\"BLOCKS\" />" +
                                        "</block>" +
                                    "</next>" +
                            "</block>" +
                        "</next>" +
                    "</block>" +
                "</xml>",
                xml);
        }

        [Fact]
        public void ToXmlGeneratesAnXmlStringForAToolboxWithOneCategory()
        {
            // Arrange
            var toolbox = new Toolbox();
            toolbox.Categories.Add(GenerateCategory("testing", true, "red",
                GenerateBlock("tahi"),
                GenerateBlock("rua")));

            // Act
            var xml = toolbox.ExportToXml(Toolbox.Format.Toolbox);

            // Assert
            Assert.Equal(
                "<toolbox><category name=\"testing\" colour=\"red\" optional=\"yes\"><block name=\"tahi\"></block><block name=\"rua\"></block></category></toolbox>",
                xml);
        }

        [Fact]
        public void ToXmlGeneratesAnXmlStringForAToolboxWithOneCategoryForBlockly()
        {
            // Arrange
            var toolbox = new Toolbox();
            toolbox.Categories.Add(GenerateCategory("testing", true, "red",
                GenerateBlock("tahi"),
                GenerateBlock("rua")));

            // Act
            var xml = toolbox.ExportToXml(Toolbox.Format.Blockly);

            // Assert
            Assert.Equal(
                "<xml>" +
                    "<block type=\"category\">" +
                        "<field name=\"NAME\">testing</field>" +
                        "<field name=\"OPTIONAL\">TRUE</field>" +
                        "<field name=\"COLOUR\">red</field>" +
                        "<statement name=\"BLOCKS\">" +
                            "<block type=\"tahi\">" +
                                "<next>" +
                                    "<block type=\"rua\" />" +
                                "</next>" +
                            "</block>" +
                        "</statement>" +
                    "</block>" +
                "</xml>",
                xml);
        }

        [Fact]
        public void ToXmlHandlesUnknownFormat()
        {
            // Arrange
            var toolbox = new Toolbox();

            // Act
            var error = Assert.Throws<Exception>(() => toolbox.ExportToXml(Toolbox.Format.Unknown));

            // Assert
            Assert.Equal(
                "Unknown format: Unknown",
                error.Message);
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