using NaoBlocks.Client.Terminal.Instructions;
using Version = NaoBlocks.Client.Terminal.Instructions.Version;

namespace NaoBlocks.Client.Terminal.Tests.Instructions
{
    public class VersionTests
    {
        private const string invalidNumberOfArguments = $"ERROR: Invalid number of arguments.:INFO::INFO: Usage is {App.ApplicationName} version <SERVER> [options]";

        [Fact]
        public void DisplayHelpTextDisplaysTheHelpText()
        {
            // Arrange
            var instruction = InitialiseInstruction();
            var console = new TestConsole();

            // Act
            instruction.DisplayHelpText(console);

            // Assert
            Assert.NotEmpty(console.Output);
        }

        [Fact]
        public async Task RunAsyncCallsClient()
        {
            // Arrange
            var instruction = InitialiseInstruction();
            var console = new TestConsole();

            // Act
            instruction.Validate(console, new[] { "help", "TestHelp" });
            var result = await instruction.RunAsync(console);

            // Assert
            Assert.Equal(0, result);
            Assert.Equal(
                new[] { "INFO:", "INFO: TestHelp: Test help instruction", "INFO:", "INFO: This is some helpful text" },
                console.Output);
        }

        [Fact]
        public async Task RunAsyncChecksItsInternalState()
        {
            // Arrange
            var instruction = new Version();
            var console = new TestConsole();

            // Act
            var error = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await instruction.RunAsync(console));

            // Assert
            Assert.Equal(
                "Instruction is in an invalid state: this should not happen when it is called from App!",
                error.Message);
        }

        [Fact]
        public void ValidateChecksItsInternalState()
        {
            // Arrange
            var instruction = new Version();
            var console = new TestConsole();

            // Act
            var error = Assert.Throws<InvalidOperationException>(
                () => instruction.Validate(console, new[] { "help" }));

            // Assert
            Assert.Equal(
                "Instruction is in an invalid state: this should not happen when it is called from App!",
                error.Message);
        }

        [Fact]
        public void ValidateChecksServerIsSet()
        {
            // Arrange
            var instruction = InitialiseInstruction();
            var console = new TestConsole();

            // Act
            var result = instruction.Validate(console, new[] { "help", "unknown" });

            // Assert
            Assert.False(result);
            Assert.Equal("ERROR: Invalid number of arguments.:INFO::INFO: Usage is NaoBlocks version <SERVER> [options]", string.Join(":", console.Output).Trim());
        }

        [Theory]
        [InlineData(false, invalidNumberOfArguments, "version")]
        [InlineData(true, "", "version", "test")]
        [InlineData(false, invalidNumberOfArguments, "help", "test", "three")]
        [InlineData(false, invalidNumberOfArguments, "help", "test", "four")]
        public void ValidateTakesOneArguments(bool allowed, string output, params string[] args)
        {
            // Arrange
            var instruction = InitialiseInstruction();
            var console = new TestConsole();

            // Act
            var result = instruction.Validate(console, args);

            // Assert
            Assert.Equal(allowed, result);
            Assert.Equal(output, string.Join(":", console.Output).Trim());
        }

        private static Version InitialiseInstruction()
        {
            var instruction = new Version
            {
                Factory = new InstructionFactory()
            };
            instruction.Factory.Initialise<TestInstruction>();
            return instruction;
        }
    }
}