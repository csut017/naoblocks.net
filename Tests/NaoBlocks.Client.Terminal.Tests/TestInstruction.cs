namespace NaoBlocks.Client.Terminal.Tests
{
    [Instruction(TestInstruction.InstructionName)]
    public class TestInstruction
        : InstructionBase
    {
        public const string InstructionName = "Test";

        public override Task<int> RunAsync(IConsole console)
        {
            throw new NotImplementedException();
        }

        public override bool Validate(IConsole console, string[] args)
        {
            throw new NotImplementedException();
        }
    }
}