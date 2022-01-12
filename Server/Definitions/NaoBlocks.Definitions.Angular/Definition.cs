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
        /// Validates the <see cref="IUIDefinition"/> instance.
        /// </summary>
        /// <param name="engine">The <see cref="IExecutionEngine"/> to use.</param>
        /// <returns>The errors from validation. Empty if there are no errors.</returns>
        public Task<IEnumerable<CommandError>> ValidateAsync(IExecutionEngine engine)
        {
            throw new NotImplementedException();
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
                { "blocks", () =>
                {
                    return string.Join($",{Environment.NewLine}", this.Blocks.Select(b => $"'{b.AstName}': {b.AstConverter}"));
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
