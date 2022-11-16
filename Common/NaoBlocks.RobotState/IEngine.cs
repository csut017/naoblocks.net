using NaoBlocks.Common;

namespace NaoBlocks.RobotState
{
    /// <summary>
    /// Defines the interface for an engine.
    /// </summary>
    public interface IEngine
    {
        /// <summary>
        /// Attempts to find a function.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <returns>The <see cref="EngineFunction"/> instance if found; null otherwise.</returns>
        EngineFunction? FindFunction(string name);

        /// <summary>
        /// Initialises the engine by loading a collection of <see cref="AstNode"/> instances.
        /// </summary>
        /// <param name="nodes">The nodes for the program.</param>
        Task InitialiseAsync(ICollection<AstNode> nodes);

        /// <summary>
        /// Initialises the engine by compiling a program.
        /// </summary>
        /// <param name="code">The program code for the engine.</param>
        Task InitialiseAsync(string code);

        /// <summary>
        /// Registers a new function.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="node">The node for the function.</param>
        void RegisterFunction(string name, IndexedNode node);

        /// <summary>
        /// Resets the engine.
        /// </summary>
        Task ResetAsync();
    }
}