namespace NaoBlocks.RobotState.Tests
{
    public class TestFunction
        : EngineFunction
    {
        public TestFunction(string name)
            : base(name)
        {
        }

        public bool WasCalled { get; private set; }

        protected override Task<EngineFunctionResult> DoExecuteAsync(IEngine engine, IndexedNode node)
        {
            this.WasCalled = true;
            return Task.FromResult(
                new EngineFunctionResult(true));
        }
    }
}