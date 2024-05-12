﻿using System.ComponentModel;
using System.Text;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NaoBlocks.Definitions.Angular
{
    /// <summary>
    /// Defines the UI components for an Angular application
    /// </summary>
    [DisplayName("Angular: Blockly")]
    [Description("Angular blockly components.")]
    public class Definition : IUIDefinition
    {
        private readonly string[] keyWords = new[] { "SYSTEM" };

        /// <summary>
        /// Gets the blocks.
        /// </summary>
        public IList<Block> Blocks { get; } = new List<Block>();

        /// <summary>
        /// Gets the AST nodes.
        /// </summary>
        public IList<AstNode> Nodes { get; } = new List<AstNode>();

        /// <summary>
        /// Generates a description of the definition.
        /// </summary>
        /// <returns>The description items.</returns>
        public Task<IEnumerable<UIDefinitionItem>> DescribeAsync()
        {
            return Task.FromResult(new[]
            {
                UIDefinitionItem.New("Blocks",
                    null,
                    this.Blocks.Select(b => UIDefinitionItem.New($"{b.Name ?? "<unknown>"} [{b.Text}]"))),
                UIDefinitionItem.New("Nodes",
                    null,
                    this.Nodes.Select(n => UIDefinitionItem.New(n.Name ?? "<unknown>"))),
            }.AsEnumerable());
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
                "blocks" => ConvertToStreamAsync(this.GenerateBlocks()),
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
            ValidateNodes(errors);
            return Task.FromResult(errors.AsEnumerable());
        }

        private static Task<Stream> ConvertToStreamAsync(string content)
        {
            return Task.FromResult(
                (Stream)new MemoryStream(Encoding.UTF8.GetBytes(content)));
        }

        private void CheckAndTidyDefinition(Block block, List<CommandError> errors, string name)
        {
            var isValid = false;
            var definition = block.Definition!.Trim();
            if (!string.IsNullOrWhiteSpace(definition))
            {
                if (keyWords.Contains(definition.ToUpperInvariant()))
                {
                    isValid = true;
                }
                else
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
            }

            if (!isValid)
            {
                errors.Add(
                    new CommandError(
                        0,
                        $"Block {name} has an invalid block definition (definition): must be valid JSON or one of [{string.Join(",", keyWords)}]"));
            }
        }

        private string GenerateBlockDefinitions()
        {
            var generators = new Dictionary<string, Func<string>>
            {
                { "blocks", () =>
                {
                    return string.Join($",{Environment.NewLine}", this.Blocks.Where(b => !"system".Equals(b.Definition, StringComparison.InvariantCultureIgnoreCase)).Select(b => b.Definition));
                }}
            };
            var output = TemplateGenerator.BuildFromTemplate<Definition>("block_definitions", generators);
            return output;
        }

        private string GenerateBlocks()
        {
            var blocks = this.Blocks.Select(b => new
            {
                name = string.IsNullOrEmpty(b.Text) ? b.Name : b.Text,
                category = b.Category ?? string.Empty,
                type = b.Name,
                toolbox = string.IsNullOrEmpty(b.Toolbox) ? $"<block type=\"{b.Name}\"></block>" : b.Toolbox,
            }).ToArray();
            var output = JsonConvert.SerializeObject(blocks, Formatting.None);
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
                    JavaScriptChecker.Check($"Block {name} has an invalid language generator (generator): must be valid JavaScript",
                        block.Generator,
                        errors);
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
                else
                {
                    JavaScriptChecker.Check($"Node {name} has an invalid converter (converter): must be valid JavaScript",
                        node.Converter,
                        errors);
                }
            }
        }
    }
}