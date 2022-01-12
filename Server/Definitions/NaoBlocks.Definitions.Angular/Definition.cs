using NaoBlocks.Common;
using NaoBlocks.Engine;

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
            throw new NotImplementedException();
        }
    }
}
