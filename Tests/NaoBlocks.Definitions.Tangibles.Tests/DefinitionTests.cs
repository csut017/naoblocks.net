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
        private const string imageRedDot = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";

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
        public async Task GenerateAsyncGeneratesBlockDefinitionsForMultipleBlocks()
        {
            // Arrange
            var definition = new Definition();
            definition.Blocks.Add(Block.New(1, "block_one", "image", "gen()"));
            var block = Block.New(new[] { 2, 3 }, "block_two", "image", "gen()");
            block.Text = "Block #2";
            definition.Blocks.Add(block);

            // Act
            var ouput = await definition.GenerateAsync("all");

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
            definition.Blocks.Add(Block.New(1, "block_one", "image", "gen"));

            // Act
            var ouput = await definition.GenerateAsync("all");

            // Assert
            using var reader = new StreamReader(ouput);
            var body = await reader.ReadToEndAsync();
            var expected = TemplateGenerator.ReadEmbededResource<DefinitionTests>("expected_single_block_definition.txt");
            Assert.Equal(expected, body);
        }

        [Fact]
        public async Task GenerateAsyncGeneratesBlockDefinitionsWithLinkedImage()
        {
            // Arrange
            var definition = new Definition();
            definition.Blocks.Add(Block.New(1, "block_one", "->image", "gen"));
            definition.Images.Add(
                new ImageDefinition { Name = "image", Image = "linked" });

            // Act
            var ouput = await definition.GenerateAsync("all");

            // Assert
            using var reader = new StreamReader(ouput);
            var body = await reader.ReadToEndAsync();
            var expected = TemplateGenerator.ReadEmbededResource<DefinitionTests>("expected_linked_image_definition.txt");
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

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Block 'test_block' (#1) does not define any TopCode numbers (numbers)",
                "Block 'test_block' (#1) does not have an image to display (image)",
                "Block 'test_block' (#1) does not have a language generator (generator)",
            }, ouput.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncChecksBlockImagesAreDataOrLinks()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            definition.Blocks.Add(Block.New(1, "block_one", "rubbish", "gen"));
            definition.Blocks.Add(Block.New(2, "block_two", imageRedDot, "gen"));
            definition.Blocks.Add(Block.New(3, "block_three", "->image", "gen"));
            definition.Images.Add(
                new ImageDefinition { Name = "image", Image = imageRedDot });

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Block 'block_one' (#1) has an invalid image (image): must be an image data URI or a link (->)",
            }, ouput.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncChecksBlockImagesLinkExists()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            definition.Blocks.Add(Block.New(1, "block_one", "->one", "gen"));
            definition.Blocks.Add(Block.New(2, "block_two", "->two", "gen"));
            definition.Images.Add(
                new ImageDefinition { Name = "one", Image = imageRedDot });

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Block 'block_two' (#2) has an invalid image (image): linked image 'two' does not exist",
            }, ouput.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncChecksForBlocks()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();

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
            definition.Blocks.Add(Block.New(1, "test_block", imageRedDot, "gen"));
            definition.Blocks.Add(Block.New(2, "test_block", imageRedDot, "gen"));

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Block 'test_block' is duplicated (#1 and #2)"
            }, ouput.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncChecksForDuplicateNumbers()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            definition.Blocks.Add(Block.New(1, "block_one", imageRedDot, "gen"));
            definition.Blocks.Add(Block.New(1, "block_two", imageRedDot, "gen"));

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Duplicate TopCode number 1 detected in 'block_two' (#2) and 'block_one' (#1)"
            }, ouput.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncChecksForDuplicateNumbersWithMultipleNumbers()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            definition.Blocks.Add(Block.New(1, "block_one", imageRedDot, "gen"));
            definition.Blocks.Add(Block.New(2, "block_two", imageRedDot, "gen"));
            definition.Blocks.Add(Block.New(new[] { 1, 2 }, "block_three", imageRedDot, "gen"));

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Duplicate TopCode number 1 detected in 'block_three' (#3) and 'block_one' (#1)",
                "Duplicate TopCode number 2 detected in 'block_three' (#3) and 'block_two' (#2)"
            }, ouput.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncChecksForNames()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            definition.Blocks.Add(Block.New(1, null, imageRedDot, "ignore"));

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Block #1 does not have a name (name)"
            }, ouput.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncChecksGeneratorIsValidJavascript()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            definition.Blocks.Add(Block.New(1, "block_one", imageRedDot, "=123"));
            definition.Blocks.Add(Block.New(2, "block_two", imageRedDot, "console.log('hello')"));

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Block 'block_one' (#1) has an invalid language generator (generator): must be valid JavaScript",
            }, ouput.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncChecksImagesHaveAName()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            definition.Blocks.Add(Block.New(1, "block_one", imageRedDot, "gen"));
            definition.Images.Add(
                new ImageDefinition { Image = imageRedDot });

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Image #1 does not have a name (name)",
            }, ouput.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncChecksImagesHaveAValidImageDefinition()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            definition.Blocks.Add(Block.New(1, "block_one", imageRedDot, "gen"));
            definition.Images.Add(
                new ImageDefinition { Name = "first", Image = "rubbish" });
            definition.Images.Add(
                new ImageDefinition { Name = "second" });

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Image 'first' (#1) has an invalid image (image): must be an image data URI",
                "Image 'second' (#2) is missing image data (image)",
            }, ouput.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncChecksImagesHaveUniqueNames()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            definition.Blocks.Add(Block.New(1, "block_one", imageRedDot, "gen"));
            definition.Images.Add(
                new ImageDefinition { Name = "first", Image = imageRedDot });
            definition.Images.Add(
                new ImageDefinition { Name = "first", Image = imageRedDot });

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Equal(new[]
            {
                "Image 'first' is duplicated (#1 and #2)"
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
                "Block #1 does not define any TopCode numbers (numbers)",
                "Block #1 does not have an image to display (image)",
                "Block #1 does not have a language generator (generator)",
            }, ouput.Select(e => e.Error).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncPassesWhenAllFieldsAreSet()
        {
            // Arrange
            var engine = new Mock<IExecutionEngine>();
            var definition = new Definition();
            definition.Blocks.Add(Block.New(1, "test_block", imageRedDot, "gen"));

            // Act
            var ouput = await definition.ValidateAsync(engine.Object);

            // Assert
            Assert.Empty(ouput);
        }
    }
}