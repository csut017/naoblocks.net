namespace NaoBlocks.RobotState.Functions
{
    /// <summary>
    /// Resets the engine.
    /// </summary>
    public class Reset
        : EngineFunction
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Reset"/> function.
        /// </summary>
        public Reset()
            : base("reset")
        {
        }

        /// <summary>
        /// Resets the engine.
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to use during execution.</param>
        /// <param name="node">The node to execute.</param>
        /// <returns>A <see cref="EngineFunctionResult"/> containing the result of the execution.</returns>
        protected override async Task<EngineFunctionResult> DoExecuteAsync(IEngine engine, IndexedNode node)
        {
            await engine.ResetAsync();
            return new EngineFunctionResult(true);
        }
    }
}