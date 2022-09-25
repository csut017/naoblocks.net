namespace NaoBlocks.RobotState
{
    /// <summary>
    /// Defines a function that the engine can perform.
    /// </summary>
    public abstract class EngineFunction
    {
        /// <summary>
        /// Initialises a new instance of <see cref="EngineFunction"/>.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        protected EngineFunction(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Executes this function.
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to use during execution.</param>
        /// <param name="node">The node to execute.</param>
        /// <returns>A <see cref="EngineFunctionResult"/> containing the result of the execution.</returns>
        public async Task<EngineFunctionResult> ExecuteAsync(IEngine engine, IndexedNode node)
        {
            try
            {
                return await this.DoExecuteAsync(engine, node);
            }
            catch (Exception ex)
            {
                return new EngineFunctionResult(ex);
            }
        }

        /// <summary>
        /// Executes the internal implementation of this function.
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to use during execution.</param>
        /// <param name="node">The node to execute.</param>
        /// <returns>A <see cref="EngineFunctionResult"/> containing the result of the execution.</returns>
        protected abstract Task<EngineFunctionResult> DoExecuteAsync(IEngine engine, IndexedNode node);
    }
}