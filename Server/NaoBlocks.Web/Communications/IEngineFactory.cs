using NaoBlocks.Engine;

namespace NaoBlocks.Web.Communications
{
    /// <summary>
    /// Factory interface for generating new <see cref="IExecutionEngine"/> instances.
    /// </summary>
    public interface IEngineFactory
    {
        /// <summary>
        /// Generate a new <see cref="IExecutionEngine"/> instance.
        /// </summary>
        /// <returns>The new <see cref="IExecutionEngine"/> and <see cref="IDatabaseSession"/>instances.</returns>
        (IExecutionEngine, IDatabaseSession) Initialise();
    }
}
