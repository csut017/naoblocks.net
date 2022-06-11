namespace NaoBlocks.Client.Terminal.Tests
{
    public partial class InstructionBaseTests
    {
        [Fact]
        public void RetrieveHelpTextRetrievesEmptyArray()
        {
            var instruction = new TestInstruction();
            Assert.Empty(instruction.RetrieveHelpText());
        }
    }
}