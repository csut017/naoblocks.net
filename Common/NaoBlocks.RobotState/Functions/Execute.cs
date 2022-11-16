namespace NaoBlocks.RobotState.Functions
{
    /// <summary>
    /// Executes a block of code.
    /// </summary>
    public class Execute
        : EngineFunction
    {
        private readonly IEnumerable<IndexedNode> nodes;

        /// <summary>
        /// Initialises a new instance of the <see cref="Execute"/> function.
        /// </summary>
        /// <param name="name">The name of the code block.</param>
        /// <param name="nodes">The nodes to execute.</param>
        public Execute(string name, IEnumerable<IndexedNode> nodes)
            : base(name)
        {
            this.nodes = nodes;
        }

        /// <summary>
        /// Executes the block of code.
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to use during execution.</param>
        /// <param name="node">The node to execute.</param>
        /// <returns>A <see cref="EngineFunctionResult"/> containing the result of the execution.</returns>
        protected override Task<EngineFunctionResult> DoExecuteAsync(IEngine engine, IndexedNode node)
        {
            throw new NotImplementedException();
        }
    }
}