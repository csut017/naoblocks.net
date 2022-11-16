namespace NaoBlocks.Client.Terminal.Tests
{
    public class InstructionFactoryTests
    {
        [Theory]
        [InlineData("", false)]
        [InlineData("ftp://server", false)]
        [InlineData("rubbish", false)]
        [InlineData("http://server", true)]
        [InlineData("https://server", true)]
        [InlineData("HTTPS://server", true)]
        public void CheckIfAddressIsValidChecksTheAddress(string serverAddress, bool isValid)
        {
            // Arrange
            var factory = new InstructionFactory();
            factory.Initialise<TestInstruction>();

            // Act
            factory.ServerAddress = serverAddress;
            var result = factory.CheckIfAddressIsValid();

            // Assert
            Assert.Equal(isValid, result);
        }

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
        public void ListRetrievesAllInstructions()
        {
            // Arrange
            var factory = new InstructionFactory();
            factory.Initialise<TestInstruction>();

            // Act
            var instructions = factory
                .List()
                .Select(i => i.Name)
                .ToArray();

            // Assert
            Assert.Equal(
                new[] { "Test", "TestHelp" },
                instructions);
        }

        [Theory]
        [InlineData("https://secure", true)]
        [InlineData("http://open", false)]
        public void RetrieveConnectionStartsAConnection(string address, bool isSecure)
        {
            // Arrange
            var factory = new InstructionFactory();
            factory.Initialise<TestInstruction>();

            // Act
            factory.ServerAddress = address;
            var connection = factory.RetrieveConnection();

            // Assert
            Assert.NotNull(connection);
            Assert.Equal(isSecure, connection.IsSecure);
        }

        [Theory]
        [InlineData("", "Server address had not been set")]
        [InlineData("ftp://server", "Invalid server address")]
        [InlineData("rubbish", "Invalid server address")]
        public void RetrieveConnectionValidatesSettings(string serverAddress, string errorMessage)
        {
            // Arrange
            var factory = new InstructionFactory();
            factory.Initialise<TestInstruction>();

            // Act
            factory.ServerAddress = serverAddress;
            var error = Assert.Throws<ConnectionException>(
                () => factory.RetrieveConnection());

            // Assert
            Assert.Equal(errorMessage, error.Message);
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

        [Fact]
        public void RetrieveSetsDescription()
        {
            // Arrange
            var factory = new InstructionFactory();
            factory.Initialise<TestInstruction>();

            // Act
            var instruction = factory.Retrieve(TestHelpInstruction.InstructionName);

            // Assert
            Assert.Equal("Test help instruction", instruction?.Description);
        }
    }
}