using Esprima;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace NaoBlocks.Definitions.Tangibles
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

        /// <summary>
        /// Generates a component from the definition.
        /// </summary>
        /// <param name="component">The component name.</param>
        /// <returns>A <see cref="Stream"/> containing the definition.</returns>
        public Task<Stream> GenerateAsync(string component)
        {
            return component.ToLowerInvariant() switch
            {
                "all" => ConvertToStreamAsync(this.GenerateAll()),
                _ => throw new ApplicationException($"Unknown content type '{component}'"),
            };
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
                    return string.Join($",", this.Blocks.Where(b => !"system".Equals(b.Definition, StringComparison.InvariantCultureIgnoreCase)).Select(b => b.Definition));
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
                else
                {
                    CheckAndTidyDefinition(block, errors, name);
                }

                if (string.IsNullOrWhiteSpace(block.Generator))
                {
                    errors.Add(new CommandError(0, $"Block {name} does not have a language generator (generator)"));
                }
                else
                {
                    try
                    {
                        var parser = new JavaScriptParser(block.Generator);
                        var program = parser.ParseScript();
                    }
                    catch
                    {
                        errors.Add(
                            new CommandError(
                                0,
                                $"Block {name} has an invalid language generator (generator): must be valid JavaScript"));
                    }
                }
            }
        }

        private void CheckAndTidyDefinition(Block block, List<CommandError> errors, string name)
        {
            var isValid = false;
            var definition = block.Definition!.Trim();
            if (!string.IsNullOrWhiteSpace(definition))
            {
                if ((definition.StartsWith("{") && definition.EndsWith("}")) || (definition.StartsWith("[") && definition.EndsWith("]")))
                {
                    try
                    {
                        var obj = JObject.Parse(definition);
                        obj["type"] = block.Name;
                        block.Definition = obj.ToString(Formatting.None);
                        isValid = true;
                    }
                    catch
                    {
                        // Don't do anything, we were just checking if the JSON was valid
                    }
                }
            }

            if (!isValid)
            {
                errors.Add(
                    new CommandError(
                        0,
                        $"Block {name} has an invalid block definition (definition): must be valid JSON"));
            }
        }
    }
}