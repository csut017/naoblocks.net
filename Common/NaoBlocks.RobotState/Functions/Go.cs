namespace NaoBlocks.RobotState.Functions
{
    /// <summary>
    /// Starts program execution.
    /// </summary>
    public class Go
        : EngineFunction
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Go"/> function.
        /// </summary>
        public Go()
            : base("go")
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
            var func = engine.FindFunction("start");
            if (func == null) throw new EngineException("start has not been defined, cannot go");
            return await func.ExecuteAsync(engine, node);
        }
    }
}