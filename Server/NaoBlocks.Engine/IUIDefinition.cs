using NaoBlocks.Common;
using NaoBlocks.Engine.Data;

namespace NaoBlocks.Engine
{
    /// <summary>
    /// Defines a UI definition.
    /// </summary>
    public interface IUIDefinition
    {
        /// <summary>
        /// Generates a description of the definition.
        /// </summary>
        /// <returns>The description items.</returns>
        Task<IEnumerable<UIDefinitionItem>> DescribeAsync();

        /// <summary>
        /// Generates a component from the definition.
        /// </summary>
        /// <param name="component">The component name.</param>
        /// <returns>A <see cref="Stream"/> containing the definition.</returns>
        Task<Stream> GenerateAsync(string component);

        /// <summary>
        /// Validates the <see cref="IUIDefinition"/> instance.
        /// </summary>
        /// <param name="engine">The <see cref="IExecutionEngine"/> to use.</param>
        /// <returns>The errors from validation. Empty if there are no errors.</returns>
        Task<IEnumerable<CommandError>> ValidateAsync(IExecutionEngine engine);
    }
}