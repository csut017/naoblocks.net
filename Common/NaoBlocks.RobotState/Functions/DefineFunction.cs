namespace NaoBlocks.RobotState.Functions
{
    /// <summary>
    /// Defines a new function.
    /// </summary>
    public class DefineFunction
        : EngineFunction
    {
        private readonly bool useName;

        /// <summary>
        /// Defines a new function with a predefined name.
        /// </summary>
        /// <param name="name">The predefined name.</param>
        public DefineFunction(string name = "function")
            : base(name)
        {
            this.useName = true;
        }

        /// <summary>
        /// Defines a new function.
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to use during execution.</param>
        /// <param name="node">The node to execute.</param>
        /// <returns>A <see cref="EngineFunctionResult"/> containing the result of the execution.</returns>
        protected override Task<EngineFunctionResult> DoExecuteAsync(IEngine engine, IndexedNode node)
        {
            if (this.useName)
            {
                throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }
    }
}