using System.ComponentModel;
using System.Text;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Data;

namespace NaoBlocks.Definitions.Tangibles
{
    /// <summary>
    /// Defines the UI components for an Angular application
    /// </summary>
    [DisplayName("Tangibles")]
    [Description("Components for using within the tangibles viewer.")]
    public class Definition : IUIDefinition
    {
        private readonly Dictionary<string, ImageDefinition> images = new Dictionary<string, ImageDefinition>();

        /// <summary>
        /// Gets the blocks.
        /// </summary>
        public IList<Block> Blocks { get; } = new List<Block>();

        /// <summary>
        /// Gets the images.
        /// </summary>
        public IList<ImageDefinition> Images { get; } = new List<ImageDefinition>();

        /// <summary>
        /// Generates a description of the definition.
        /// </summary>
        /// <returns>The description items.</returns>
        public Task<IEnumerable<UIDefinitionItem>> DescribeAsync()
        {
            this.InitialiseImages();
            Func<Block, string?> mapImage = (block) =>
            {
                if (!string.IsNullOrEmpty(block.Image) && block.Image.StartsWith("->"))
                {
                    return this.images[block.Image[2..]].Image;
                }
                return block.Image;
            };
            return Task.FromResult(new[]
            {
                UIDefinitionItem.New("Blocks",
                    null,
                    this.Blocks.Select(b => new UIDefinitionItem{
                        Name = b.Name ?? "<unknown>",
                        Image = mapImage(b),
                    })),
                UIDefinitionItem.New("Images",
                    null,
                    this.Images.Select(i => new UIDefinitionItem{
                        Name = i.Name ?? "<unknown>",
                        Image = i.Image,
                    })),
            }.AsEnumerable());
        }

        /// <summary>
        /// Generates a component from the definition.
        /// </summary>
        /// <param name="component">The component name.</param>
        /// <returns>A <see cref="Stream"/> containing the definition.</returns>
        public Task<Stream> GenerateAsync(string component)
        {
            this.InitialiseImages();
            return component.ToLowerInvariant() switch
            {
                "all" => ConvertToStreamAsync(this.GenerateAll()),
                _ => throw new ApplicationException($"Unknown content type '{component}'"),
            };
        }

        /// <summary>
        /// Validates the <see cref="IUIDefinition"/> instance.
        /// </summary>
        /// <param name="engine">The <see cref="IExecutionEngine"/> to use.</param>
        /// <returns>The errors from validation. Empty if there are no errors.</returns>
        public Task<IEnumerable<CommandError>> ValidateAsync(IExecutionEngine engine)
        {
            var errors = new List<CommandError>();
            ValidateBlocks(errors);
            return Task.FromResult(errors.AsEnumerable());
        }

        private static Task<Stream> ConvertToStreamAsync(string content)
        {
            return Task.FromResult(
                (Stream)new MemoryStream(Encoding.UTF8.GetBytes(content)));
        }

        private string GenerateAll()
        {
            var generators = new Dictionary<string, Func<string>>
            {
                { "blocks", () =>
                {
                    return string.Join(
                        ",",
                        this.Blocks.SelectMany(b => b.Numbers.Select(n => GenerateBlockDefinition(b, n))));
                }},
                { "language", () =>
                {
                    return string.Join(
                        $"{Environment.NewLine}",
                        this.Blocks.Select(b => $"Tangibles.NaoLang.{b.Name} = function (block) {{{Environment.NewLine}{b.Generator}{Environment.NewLine}}};"));
                }}
            };
            var output = TemplateGenerator.BuildFromTemplate<Definition>("all", generators);
            return output;
        }

        /// <summary>
        /// Generates the block definition to include in the template.
        /// </summary>
        /// <param name="block">The <see cref="Block"/> to generate the definition for.</param>
        /// <param name="number">The topcode number for the block</param>
        /// <returns>The textual definition of the <see cref="Block"/>.</returns>
        private string GenerateBlockDefinition(Block block, int number)
        {
            var builder = new StringBuilder();
            builder.Append("{\"number\":");
            builder.Append(number);
            builder.Append(",\"type\":\"");
            builder.Append(block.Name);
            builder.Append("\",\"text\":\"");
            builder.Append(block.Text ?? block.Name);
            builder.Append("\",\"image\":\"");
            var image = block.Image ?? String.Empty;
            if (image.StartsWith("->")) image = this.images[image[2..]].Image;
            builder.Append(image);
            builder.Append("\"}");
            return builder.ToString();
        }

        private void InitialiseImages()
        {
            this.images.Clear();
            foreach (var image in this.Images)
            {
                this.images[image.Name!] = image;
            }
        }

        /// <summary>
        /// Performs the validation checks on the blocks.
        /// </summary>
        /// <param name="errors">The destination for any errors.</param>
        private void ValidateBlocks(List<CommandError> errors)
        {
            if (!this.Blocks.Any())
            {
                errors.Add(new CommandError(0, "Definition does not contain any blocks"));
                return;
            }

            var imageKeys = new Dictionary<string, int>();
            var index = 0;
            foreach (var image in this.Images)
            {
                index++;
                var name = $"#{index}";
                if (string.IsNullOrEmpty(image.Name))
                {
                    errors.Add(new CommandError(0, $"Image {name} does not have a name (name)"));
                }
                else
                {
                    name = $"'{image.Name}' (#{index})";
                    if (imageKeys.TryGetValue(image.Name, out var key))
                    {
                        errors.Add(new CommandError(0, $"Image '{image.Name}' is duplicated (#{key} and #{index})"));
                    }
                    else
                    {
                        imageKeys.Add(image.Name, index);
                    }
                }

                if (string.IsNullOrEmpty(image.Image))
                {
                    errors.Add(new CommandError(0, $"Image {name} is missing image data (image)"));
                }
                else if (!image.Image.StartsWith("data:image/png;base64,"))
                {
                    errors.Add(new CommandError(0, $"Image {name} has an invalid image (image): must be an image data URI"));
                }
            }

            index = 0;
            var blockIndex = new Dictionary<string, int>();
            var numberIndex = new Dictionary<int, string>();
            foreach (var block in this.Blocks)
            {
                index++;
                var name = $"#{index}";
                if (string.IsNullOrWhiteSpace(block.Name))
                {
                    errors.Add(new CommandError(0, $"Block {name} does not have a name (name)"));
                }
                else
                {
                    name = $"'{block.Name}' (#{index})";
                    if (blockIndex.TryGetValue(block.Name, out var previous))
                    {
                        errors.Add(new CommandError(0, $"Block '{block.Name}' is duplicated (#{previous} and #{index})"));
                    }
                    else
                    {
                        blockIndex.Add(block.Name, index);
                    }
                }

                if (!block.Numbers.Any())
                {
                    errors.Add(new CommandError(0, $"Block {name} does not define any TopCode numbers (numbers)"));
                }
                else
                {
                    foreach (var number in block.Numbers)
                    {
                        if (numberIndex.TryGetValue(number, out var previous))
                        {
                            errors.Add(new CommandError(0, $"Duplicate TopCode number {number} detected in {name} and {previous}"));
                        }
                        else
                        {
                            numberIndex.Add(number, name);
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(block.Image))
                {
                    errors.Add(new CommandError(0, $"Block {name} does not have an image to display (image)"));
                }
                else
                {
                    var image = block.Image;
                    var isLink = image.StartsWith("->");
                    if (!(isLink || image.StartsWith("data:image/png;base64,")))
                    {
                        errors.Add(new CommandError(0, $"Block {name} has an invalid image (image): must be an image data URI or a link (->)"));
                    }
                    else if (isLink)
                    {
                        var linkName = image[2..];
                        if (!imageKeys.ContainsKey(linkName))
                        {
                            errors.Add(new CommandError(0, $"Block {name} has an invalid image (image): linked image '{linkName}' does not exist"));
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(block.Generator))
                {
                    errors.Add(new CommandError(0, $"Block {name} does not have a language generator (generator)"));
                }
                else
                {
                    JavaScriptChecker.Check($"Block {name} has an invalid language generator (generator): must be valid JavaScript",
                        block.Generator,
                        errors);
                }
            }
        }
    }
}