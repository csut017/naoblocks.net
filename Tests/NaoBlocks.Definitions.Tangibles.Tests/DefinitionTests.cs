using Moq;
using NaoBlocks.Engine;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Definitions.Tangibles.Tests
{
    public class DefinitionTests
    {
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

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Block #1 does not have a name (name)"
            }, ouput.Select(e => e.Error).ToArray());
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
        public async Task ValidateAsyncHandlesAllFieldsMissing()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            definition.Blocks.Add(new Block
            {
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

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Empty(ouput);
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

            // Act
            await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal("{\"type\":\"test_block\"}", block.Definition);
        }

        [Fact]
        public async Task ValidateAsyncChecksDefintionsAreJson()
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

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Block 'block_one' (#1) has an invalid block definition (definition): must be valid JSON",
                "Block 'block_three' (#3) has an invalid block definition (definition): must be valid JSON",
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
                Definition = "{}",
                Generator = "=123"
            });
            definition.Blocks.Add(new Block
            {
                Name = "block_two",
                Definition = "{}",
                Generator = "console.log('hello')"
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

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Block 'test_block' is duplicated (#1 and #2)"
            }, ouput.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task GenerateAsyncGeneratesBlockDefinitionsForSingleBlock()
        {
            // Arrange
            var definition = new Definition();
            definition.Blocks.Add(new Block
            {
                Name = "robot_wait",
                Definition = "{\"id\":1}",
                Generator = "return 'wait()';"
            });

            // Act
            var ouput = await definition.GenerateAsync("all");

            // Assert
            using var reader = new StreamReader(ouput);
            var body = await reader.ReadToEndAsync();
            var expected = TemplateGenerator.ReadEmbededResource<DefinitionTests>("expected_single_block_definition.txt");
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
                Definition = "{\"id\":1}",
                Generator = "return 'wait()';"
            });
            definition.Blocks.Add(new Block
            {
                Name = "robot_rest",
                Definition = "{\"id\":2}",
                Generator = "return 'rest()';"
            });

            // Act
            var ouput = await definition.GenerateAsync("all");

            // Assert
            using var reader = new StreamReader(ouput);
            var body = await reader.ReadToEndAsync();
            var expected = TemplateGenerator.ReadEmbededResource<DefinitionTests>("expected_multiple_block_definition.txt");
            Assert.Equal(expected, body);
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
    }
}