namespace NaoBlocks.Client.Terminal.Tests
{
    public class InstructionAttributeTests
    {
        private const string instructionName = "tohutohu";

        [Fact]
        public void ConstructorInitialisesName()
        {
            var attrib = new InstructionAttribute(instructionName);
            Assert.Equal(instructionName, attrib.Name);
        }
    }
}