namespace NaoBlocks.Client.Terminal.Tests
{
    [Instruction(TestInstruction.InstructionName)]
    public class TestInstruction
        : InstructionBase
    {
        public const string InstructionName = "Test";

        public Func<int> OnRunAsync { get; internal set; } = () => 1;

        public Func<string[], bool> OnValidate { get; internal set; } = _ => true;

        public bool RunAsyncCalled { get; private set; }

        public bool ValidateCalled { get; private set; }

        public (string[], IDictionary<string, string[]>) ParseArgs(string[] args)
        {
            return ParseNamedArgs(args);
        }

        public override Task<int> RunAsync(IConsole console)
        {
            this.RunAsyncCalled = true;
            return Task.FromResult(
                this.OnRunAsync());
        }

        public override bool Validate(IConsole console, string[] args)
        {
            this.ValidateCalled = true;
            return this.OnValidate(args);
        }
    }
}