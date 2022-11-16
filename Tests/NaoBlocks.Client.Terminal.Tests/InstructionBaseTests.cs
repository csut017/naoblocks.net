namespace NaoBlocks.Client.Terminal.Tests
{
    public partial class InstructionBaseTests
    {
        [Fact]
        public void DisplayHelpTextDisplaysNothing()
        {
            var instruction = new TestInstruction();
            var console = new TestConsole();
            instruction.DisplayHelpText(console);
            Assert.Empty(console.Output);
        }
    }
}