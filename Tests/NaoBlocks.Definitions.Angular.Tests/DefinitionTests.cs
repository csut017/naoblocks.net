using Moq;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Definitions.Angular.Tests
{
    public class DefinitionTests
    {
        [Fact]
        public async Task DescribeAsyncContainsBaseItems()
        {
            // Arrange
            var definition = new Definition();

            // Act
            var description = await definition.DescribeAsync();

            // Assert
            Assert.Equal(
                new[] { "Blocks", "Nodes" },
                description.Select(c => c.Name).ToArray());
        }

        [Fact]
        public async Task DescribeAsyncContainsBlocks()
        {
            // Arrange
            var definition = new Definition();
            definition.Blocks.Add(new Block());

            // Act
            var description = await definition.DescribeAsync();

            // Assert
            var child = description.SingleOrDefault(c => c.Name == "Blocks");
            Assert.NotEmpty(child?.Children ?? new List<UIDefinitionItem>());
        }

        [Fact]
        public async Task DescribeAsyncContainsNodes()
        {
            // Arrange
            var definition = new Definition();
            definition.Nodes.Add(new AstNode());

            // Act
            var description = await definition.DescribeAsync();

            // Assert
            var child = description.SingleOrDefault(c => c.Name == "Nodes");
            Assert.NotEmpty(child?.Children ?? new List<UIDefinitionItem>());
        }

        [Fact]
        public async Task GenerateAsyncFailsWithUnknown()
        {
            // Arrange
            var definition = new Definition();

            // Act
            var error = await Assert.ThrowsAsync<ApplicationException>(async () => await definition.GenerateAsync("rubbish"));

            // Assert
            Assert.Equal("Unknown content type 'rubbish'", error.Message);
        }

        [Fact]
        public async Task GenerateAsyncGeneratesAstConversions()
        {
            // Arrange
            var definition = new Definition();
            definition.Nodes.Add(new AstNode
            {
                Name = "wait",
                Converter = "new BlockDefinition('robot_wait', new ValueDefinition('TIME'))"
            });

            // Act
            var ouput = await definition.GenerateAsync("conversions");

            // Assert
            using var reader = new StreamReader(ouput);
            var body = await reader.ReadToEndAsync();
            var expected = TemplateGenerator.ReadEmbededResource<DefinitionTests>("expected_conversions.txt");
            Assert.Equal(expected, body);
        }

        [Fact]
        public async Task GenerateAsyncGeneratesBlockDefinitionsForMultipleBlocks()
        {
            // Arrange
            var definition = new Definition();
            definition.Blocks.Add(new Block
            {
                Name = "robot_wait",
                Definition = "{\"type\": \"robot_wait\",\"message0\": \"Wait for %1s\",\"args0\": [{\"type\": \"input_value\",\"check\": \"Number\",\"name\": \"TIME\"}],\"nextStatement\": null,\"previousStatement\": null,\"colour\": 65,\"tooltip\": \"Puts the robot in a safe resting position.\"}"
            });
            definition.Blocks.Add(new Block
            {
                Name = "robot_rest",
                Definition = "{\"type\": \"robot_rest\",\"message0\": \"Rest\",\"nextStatement\": null,\"previousStatement\": null,\"colour\": 65,\"tooltip\": \"Puts the robot in a safe resting position.\"}"
            });

            // Act
            var ouput = await definition.GenerateAsync("block_definitions");

            // Assert
            using var reader = new StreamReader(ouput);
            var body = await reader.ReadToEndAsync();
            var expected = TemplateGenerator.ReadEmbededResource<DefinitionTests>("expected_multiple_block_definition.txt");
            Assert.Equal(expected, body);
        }

        [Fact]
        public async Task GenerateAsyncGeneratesBlockDefinitionsForSingleBlock()
        {
            // Arrange
            var definition = new Definition();
            definition.Blocks.Add(new Block
            {
                Name = "robot_wait",
                Definition = "{\"type\": \"robot_wait\",\"message0\": \"Wait for %1s\",\"args0\": [{\"type\": \"input_value\",\"check\": \"Number\",\"name\": \"TIME\"}],\"nextStatement\": null,\"previousStatement\": null,\"colour\": 65,\"tooltip\": \"Puts the robot in a safe resting position.\"}"
            });

            // Act
            var ouput = await definition.GenerateAsync("block_definitions");

            // Assert
            using var reader = new StreamReader(ouput);
            var body = await reader.ReadToEndAsync();
            var expected = TemplateGenerator.ReadEmbededResource<DefinitionTests>("expected_single_block_definition.txt");
            Assert.Equal(expected, body);
        }

        [Fact]
        public async Task GenerateAsyncGeneratesBlocks()
        {
            // Arrange
            var definition = new Definition();
            definition.Blocks.Add(new Block
            {
                Name = "wait"
            });
            definition.Blocks.Add(new Block
            {
                Name = "control_if",
                Category = "System"
            });
            definition.Blocks.Add(new Block
            {
                Name = "wait",
                Text = "Do nothing"
            });
            definition.Blocks.Add(new Block
            {
                Name = "robot_say",
                Category = "Robot Actions",
                Text = "Say",
                Toolbox = "<block type=\"robot_say\"><value name=\"TEXT\"><shadow type=\"text\"><field name=\"TEXT\">abc</field></shadow></value></block>"
            });

            // Act
            var ouput = await definition.GenerateAsync("blocks");

            // Assert
            using var reader = new StreamReader(ouput);
            var body = await reader.ReadToEndAsync();
            var expected = TemplateGenerator.ReadEmbededResource<DefinitionTests>("expected_blocks.txt");
            Assert.Equal(expected, body);
        }

        [Fact]
        public async Task GenerateAsyncGeneratesLanguage()
        {
            // Arrange
            var definition = new Definition();
            definition.Blocks.Add(new Block
            {
                Name = "robot_wait",
                Generator = "var time = Blockly.NaoLang.valueToCode(block, 'TIME', Blockly.NaoLang.ORDER_ATOMIC);var code = 'wait(' + time + ')\\n';return Blockly.NaoLang.generatePrefix() + code;"
            });

            // Act
            var ouput = await definition.GenerateAsync("language");

            // Assert
            using var reader = new StreamReader(ouput);
            var body = await reader.ReadToEndAsync();
            var expected = TemplateGenerator.ReadEmbededResource<DefinitionTests>("expected_language.txt");
            Assert.Equal(expected, body);
        }

        [Fact]
        public async Task GenerateAsyncIgnoresSystemBlocks()
        {
            // Arrange
            var definition = new Definition();
            definition.Blocks.Add(new Block
            {
                Name = "robot_wait",
                Definition = "{\"type\": \"robot_wait\",\"message0\": \"Wait for %1s\",\"args0\": [{\"type\": \"input_value\",\"check\": \"Number\",\"name\": \"TIME\"}],\"nextStatement\": null,\"previousStatement\": null,\"colour\": 65,\"tooltip\": \"Puts the robot in a safe resting position.\"}"
            });
            definition.Blocks.Add(new Block
            {
                Name = "robot_rest",
                Definition = "SYSTEM"
            });

            // Act
            var ouput = await definition.GenerateAsync("block_definitions");

            // Assert
            using var reader = new StreamReader(ouput);
            var body = await reader.ReadToEndAsync();
            var expected = TemplateGenerator.ReadEmbededResource<DefinitionTests>("expected_single_block_definition.txt");
            Assert.Equal(expected, body);
        }

        [Fact]
        public async Task ValidateAsyncChecksAllFields()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            definition.Blocks.Add(new Block
            {
                Name = "test_block"
            });
            definition.Nodes.Add(new AstNode
            {
                Name = "node",
                Converter = "con"
            });

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Block 'test_block' (#1) does not have a block definition (definition)",
                "Block 'test_block' (#1) does not have a language generator (generator)",
            }, ouput.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncChecksDefintionsAreJsonOrKeyword()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            definition.Blocks.Add(new Block
            {
                Name = "block_one",
                Definition = "random",
                Generator = "gen"
            });
            definition.Blocks.Add(new Block
            {
                Name = "block_two",
                Definition = "{\"type\":\"test_block\"}",
                Generator = "gen"
            });
            definition.Blocks.Add(new Block
            {
                Name = "block_three",
                Definition = "SYSTEM",
                Generator = "gen"
            });
            definition.Nodes.Add(new AstNode
            {
                Name = "node",
                Converter = "con"
            });

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Block 'block_one' (#1) has an invalid block definition (definition): must be valid JSON or one of [SYSTEM]",
            }, ouput.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncChecksForBlocks()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            definition.Nodes.Add(new AstNode
            {
                Name = "node",
                Converter = "con"
            });

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Definition does not contain any blocks"
            }, ouput.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncChecksForDuplicateBlocks()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            definition.Blocks.Add(new Block
            {
                Name = "test_block",
                Definition = "{}",
                Generator = "gen"
            });
            definition.Blocks.Add(new Block
            {
                Name = "test_block",
                Definition = "{}",
                Generator = "gen"
            });
            definition.Nodes.Add(new AstNode
            {
                Name = "node",
                Converter = "con"
            });

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Block 'test_block' is duplicated (#1 and #2)"
            }, ouput.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncChecksForDuplicateNode()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            definition.Blocks.Add(new Block
            {
                Name = "test_block",
                Definition = "{}",
                Generator = "gen"
            });
            definition.Nodes.Add(new AstNode
            {
                Name = "node",
                Converter = "first"
            });
            definition.Nodes.Add(new AstNode
            {
                Name = "node",
                Converter = "second"
            });

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Node 'node' is duplicated (#1 and #2)"
            }, ouput.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncChecksForNames()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            definition.Blocks.Add(new Block
            {
                Definition = "{}",
                Generator = "gen"
            });
            definition.Nodes.Add(new AstNode
            {
                Name = "node",
                Converter = "con"
            });

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Block #1 does not have a name (name)"
            }, ouput.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncChecksForNodes()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            definition.Blocks.Add(new Block
            {
                Name = "test_block",
                Definition = "{}",
                Generator = "gen"
            });

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Definition does not contain any nodes"
            }, ouput.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncChecksGeneratorIsValidJavascript()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            definition.Blocks.Add(new Block
            {
                Name = "block_one",
                Definition = "SYSTEM",
                Generator = "=123"
            });
            definition.Blocks.Add(new Block
            {
                Name = "block_two",
                Definition = "SYSTEM",
                Generator = "console.log('hello')"
            });
            definition.Nodes.Add(new AstNode
            {
                Name = "node",
                Converter = "con"
            });

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Block 'block_one' (#1) has an invalid language generator (generator): must be valid JavaScript",
            }, ouput.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncChecksMissingFields()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            definition.Blocks.Add(new Block
            {
                Name = "test_block",
                Definition = "{}",
                Generator = "gen"
            });
            definition.Nodes.Add(new AstNode
            {
                Name = "ast"
            });

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Node 'ast' (#1) does not have a converter (converter)",
            }, ouput.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncChecksNodeConverterIsValidJavascript()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            definition.Blocks.Add(new Block
            {
                Name = "test_block",
                Definition = "{}",
                Generator = "gen"
            });
            definition.Nodes.Add(new AstNode
            {
                Name = "ast_1",
                Converter = "gen"
            });
            definition.Nodes.Add(new AstNode
            {
                Name = "ast_2",
                Converter = "=gen"
            });

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Node 'ast_2' (#2) has an invalid converter (converter): must be valid JavaScript",
            }, ouput.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncChecksNodeNames()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            definition.Blocks.Add(new Block
            {
                Name = "test_block",
                Definition = "{}",
                Generator = "gen"
            });
            definition.Nodes.Add(new AstNode());

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Node #1 does not have a name (name)",
                "Node #1 does not have a converter (converter)",
            }, ouput.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncFillsInDefinitionTypes()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            var block = new Block
            {
                Name = "test_block",
                Definition = "{}",
                Generator = "gen"
            };
            definition.Blocks.Add(block);
            definition.Nodes.Add(new AstNode
            {
                Name = "node",
                Converter = "con"
            });

            // Act
            await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal("{\"type\":\"test_block\"}", block.Definition);
        }

        [Fact]
        public async Task ValidateAsyncHandlesAllFieldsMissing()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            definition.Blocks.Add(new Block
            {
            });
            definition.Nodes.Add(new AstNode
            {
                Name = "node",
                Converter = "con"
            });

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Block #1 does not have a name (name)",
                "Block #1 does not have a block definition (definition)",
                "Block #1 does not have a language generator (generator)",
            }, ouput.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncPassesWhenAllFieldsAreSet()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            definition.Blocks.Add(new Block
            {
                Name = "test_block",
                Definition = "{}",
                Generator = "gen"
            });
            definition.Nodes.Add(new AstNode
            {
                Name = "node",
                Converter = "con"
            });

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Empty(ouput);
        }
    }
}