namespace NaoBlocks.Client.Terminal.Tests
{
    public class InstructionFactoryTests
    {
        [Fact]
        public void InitialiseCanOnlyBeCalledOnce()
        {
            // Arrange
            var factory = new InstructionFactory();
            factory.Initialise<TestInstruction>();

            // Act and assert
            Assert.Throws<InstructionFactoryException>(
                () => factory.Initialise<InstructionBase>());
        }

        [Fact]
        public void RetrieveHandlesFailingInstruction()
        {
            // Arrange
            var factory = new InstructionFactory();
            factory.Initialise<TestInstruction>();

            // Act and assert
            Assert.Throws<InstructionFactoryException>(
            () => factory.Retrieve("failing"));
        }

        [Fact]
        public void RetrieveHandlesFailsIfNotInitialised()
        {
            // Arrange
            var factory = new InstructionFactory();

            // Act and assert
            Assert.Throws<InstructionFactoryException>(
                () => factory.Retrieve("test"));
        }

        [Fact]
        public void RetrieveHandlesMissingInstruction()
        {
            // Arrange
            var factory = new InstructionFactory();
            factory.Initialise<TestInstruction>();

            // Act
            var instruction = factory.Retrieve("missing");

            // Assert
            Assert.Null(instruction);
        }

        [Theory]
        [InlineData("test")]
        [InlineData("Test")]
        [InlineData("TEST")]
        public void RetrieveHandlesRetrievesInstruction(string name)
        {
            // Arrange
            var factory = new InstructionFactory();
            factory.Initialise<TestInstruction>();

            // Act
            var instruction = factory.Retrieve(name);

            // Assert
            Assert.NotNull(instruction);
            Assert.Equal(TestInstruction.InstructionName, instruction?.Name);
        }
    }
}