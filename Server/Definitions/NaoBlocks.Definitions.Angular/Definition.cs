using NaoBlocks.Common;
using NaoBlocks.Engine;
using System.Text;

namespace NaoBlocks.Definitions.Angular
{
    /// <summary>
    /// Defines the UI components for an Angular application
    /// </summary>
    public class Definition : IUIDefinition
    {
        /// <summary>
        /// Gets the blocks.
        /// </summary>
        public IList<Block> Blocks { get; } = new List<Block>();

        /// <summary>
        /// Gets the AST nodes.
        /// </summary>
        public IList<AstNode> Nodes { get; } = new List<AstNode>();

        /// <summary>
        /// Validates the <see cref="IUIDefinition"/> instance.
        /// </summary>
        /// <param name="engine">The <see cref="IExecutionEngine"/> to use.</param>
        /// <returns>The errors from validation. Empty if there are no errors.</returns>
        public Task<IEnumerable<CommandError>> ValidateAsync(IExecutionEngine engine)
        {
            var errors = new List<CommandError>();

            ValidateBlocks(errors);
            ValidateNodes(errors);
            return Task.FromResult(errors.AsEnumerable());
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

            var index = 0;
            var blockIndex = new Dictionary<string, int>();
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

                if (string.IsNullOrWhiteSpace(block.Definition))
                {
                    errors.Add(new CommandError(0, $"Block {name} does not have a block definition (definition)"));
                }

                if (string.IsNullOrWhiteSpace(block.Generator))
                {
                    errors.Add(new CommandError(0, $"Block {name} does not have a language generator (generator)"));
                }
            }
        }

        /// <summary>
        /// Performs the validation checks on the nodes.
        /// </summary>
        /// <param name="errors">The destination for any errors.</param>
        private void ValidateNodes(List<CommandError> errors)
        {
            if (!this.Nodes.Any())
            {
                errors.Add(new CommandError(0, "Definition does not contain any nodes"));
                return;
            }

            var index = 0;
            var nodeIndex = new Dictionary<string, int>();
            foreach (var node in this.Nodes)
            {
                index++;
                var name = $"#{index}";
                if (string.IsNullOrWhiteSpace(node.Name))
                {
                    errors.Add(new CommandError(0, $"Node {name} does not have a name (name)"));
                }
                else
                {
                    name = $"'{node.Name}' (#{index})";
                    if (nodeIndex.TryGetValue(node.Name, out var previous))
                    {
                        errors.Add(new CommandError(0, $"Node '{node.Name}' is duplicated (#{previous} and #{index})"));
                    }
                    else
                    {
                        nodeIndex.Add(node.Name, index);
                    }
                }

                if (string.IsNullOrWhiteSpace(node.Converter))
                {
                    errors.Add(new CommandError(0, $"Node {name} does not have a converter (converter)"));
                }
            }
        }

        /// <summary>
        /// Generates a component from the definition.
        /// </summary>
        /// <param name="component">The component name.</param>
        /// <returns>A <see cref="Stream"/> containing the definition.</returns>
        public Task<Stream> GenerateAsync(string component)
        {
            return component.ToLowerInvariant() switch
            {
                "block_definitions" => ConvertToStreamAsync(this.GenerateBlockDefinitions()),
                "conversions" => ConvertToStreamAsync(this.GenerateConversions()),
                "language" => ConvertToStreamAsync(this.GenerateLanguage()),
                _ => throw new ApplicationException($"Unknown content type '{component}'"),
            };
        }

        private static Task<Stream> ConvertToStreamAsync(string content)
        {
            return Task.FromResult(
                (Stream)new MemoryStream(Encoding.UTF8.GetBytes(content)));
        }

        private string GenerateBlockDefinitions()
        {
            var generators = new Dictionary<string, Func<string>>
            {
                { "blocks", () =>
                {
                    return string.Join($",{Environment.NewLine}", this.Blocks.Select(b => b.Definition));
                }}
            };
            var output = TemplateGenerator.BuildFromTemplate<Definition>("block_definitions", generators);
            return output;
        }

        private string GenerateConversions()
        {
            var generators = new Dictionary<string, Func<string>>
            {
                { "nodes", () =>
                {
                    return string.Join($",{Environment.NewLine}", this.Nodes.Select(n => $"'{n.Name}': {n.Converter}"));
                }}
            };
            var output = TemplateGenerator.BuildFromTemplate<Definition>("conversions", generators);
            return output;
        }

        private string GenerateLanguage()
        {
            var generators = new Dictionary<string, Func<string>>
            {
                { "blocks", () =>
                {
                    return string.Join(
                        $"{Environment.NewLine}",
                        this.Blocks.Select(b => $"Blockly.NaoLang.{b.Name} = function (block) {{{Environment.NewLine}{b.Generator}{Environment.NewLine}}};"));
                }}
            };
            var output = TemplateGenerator.BuildFromTemplate<Definition>("language", generators);
            return output;
        }
    }
}
