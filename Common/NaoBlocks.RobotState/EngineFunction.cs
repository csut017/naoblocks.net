namespace NaoBlocks.RobotState
{
    /// <summary>
    /// Defines a function that the engine can perform.
    /// </summary>
    public class EngineFunction
    {
        private readonly Func<Engine, Task<EngineFunctionResult>> function;

        /// <summary>
        /// Initialises a new instance of <see cref="EngineFunction"/>.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="function">The function to execute.</param>
        public EngineFunction(string name, Func<Engine, Task<EngineFunctionResult>> function)
        {
            this.Name = name;
            this.function = function;
        }

        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Executes this function.
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to use during execution.</param>
        /// <returns>A <see cref="EngineFunctionResult"/> containing the result of the execution.</returns>
        public async Task<EngineFunctionResult> ExecuteAsync(Engine engine)
        {
            return await function(engine);
        }
    }
}