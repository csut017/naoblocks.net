namespace NaoBlocks.Client.Terminal.Tests
{
    [Instruction(TestHelpInstruction.InstructionName, "Test help instruction")]
    public class TestHelpInstruction
        : InstructionBase
    {
        public const string InstructionName = "TestHelp";

        public override void DisplayHelpText(IConsole console)
        {
            console.WriteMessage("This is some helpful text");
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