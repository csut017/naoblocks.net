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
        private const string imageQuestion = "data:image/png;base64,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";
        private const string imageRedDot = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";

        [Fact]
        public async Task DescribeAsyncContainsBaseItems()
        {
            // Arrange
            var definition = new Definition();

            // Act
            var description = await definition.DescribeAsync();

            // Assert
            Assert.Equal(
                new[] { "Blocks", "Images" },
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
            Assert.NotEmpty(child?.Children);
        }

        [Fact]
        public async Task DescribeAsyncContainsImages()
        {
            // Arrange
            var definition = new Definition();
            definition.Images.Add(new ImageDefinition());

            // Act
            var description = await definition.DescribeAsync();

            // Assert
            var child = description.SingleOrDefault(c => c.Name == "Images");
            Assert.NotEmpty(child?.Children);
        }

        [Fact]
        public async Task DescribeAsyncContainsImagesData()
        {
            // Arrange
            var definition = new Definition();
            definition.Images.Add(new ImageDefinition
            {
                Name = "dot",
                Image = imageRedDot
            });
            definition.Blocks.Add(new Block { Image = "->dot", Name = "dot" });
            definition.Blocks.Add(new Block { Image = imageQuestion, Name = "question" });

            // Act
            var description = await definition.DescribeAsync();

            // Assert
            var images = description.SelectMany(
                n => n.Children.Select(c => $"{n.Name}.{c.Name}={c.Image}"))
                .OrderBy(n => n)
                .ToArray();
            Assert.Equal(
                new[]
                {
                    $"Blocks.dot={imageRedDot}",
                    $"Blocks.question={imageQuestion}",
                    $"Images.dot={imageRedDot}",
                },
                images);
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