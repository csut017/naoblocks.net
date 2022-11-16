namespace NaoBlocks.Client.Terminal.Tests
{
    [Instruction("Failing")]
    public class FailingInstruction
        : InstructionBase
    {
        public FailingInstruction()
        {
            throw new Exception();
        }

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