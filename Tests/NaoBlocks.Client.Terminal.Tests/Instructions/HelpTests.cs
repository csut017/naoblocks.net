using NaoBlocks.Client.Terminal.Instructions;

namespace NaoBlocks.Client.Terminal.Tests.Instructions
{
    public class HelpTests
    {
        private const string invalidNumberOfArguments = $"ERROR: Invalid number of arguments.:INFO::INFO: Usage is {App.ApplicationName} help [instruction]";

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
        public async Task RunAsyncChecksItsInternalState()
        {
            // Arrange
            var instruction = new Help();
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
        public async Task RunAsyncIncludesHelpText()
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
        public async Task RunAsyncListsAllAvailableInstructions()
        {
            // Arrange
            var instruction = InitialiseInstruction();
            var console = new TestConsole();

            // Act
            var result = await instruction.RunAsync(console);

            // Assert
            Assert.Equal(0, result);
            Assert.Equal(
                new[] {
                    "INFO:",
                    "INFO: Usage: NaoBlocks [instruction]",
                    "INFO:",
                    "INFO: Instructions:",
                    "INFO: \tTest",
                    "INFO: \tTestHelp: Test help instruction",
                    "INFO:",
                    "INFO: Use NaoBlocks help [instruction] to display the options for an instruction"
                },
                console.Output);
        }

        [Fact]
        public void ValidateCheckInstructionName()
        {
            // Arrange
            var instruction = InitialiseInstruction();
            var console = new TestConsole();

            // Act
            var result = instruction.Validate(console, new[] { "help", "unknown" });

            // Assert
            Assert.False(result);
            Assert.Equal("ERROR: Unknown instruction 'unknown'", string.Join(":", console.Output).Trim());
        }

        [Fact]
        public void ValidateChecksItsInternalState()
        {
            // Arrange
            var instruction = new Help();
            var console = new TestConsole();

            // Act
            var error = Assert.Throws<InvalidOperationException>(
                () => instruction.Validate(console, new[] { "help" }));

            // Assert
            Assert.Equal(
                "Instruction is in an invalid state: this should not happen when it is called from App!",
                error.Message);
        }

        [Theory]
        [InlineData(true, "", "help")]
        [InlineData(true, "", "help", "test")]
        [InlineData(false, invalidNumberOfArguments, "help", "test", "three")]
        [InlineData(false, invalidNumberOfArguments, "help", "test", "four")]
        public void ValidateTakesOneOrZeroArguments(bool allowed, string output, params string[] args)
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

        private static Help InitialiseInstruction()
        {
            var instruction = new Help
            {
                Factory = new InstructionFactory()
            };
            instruction.Factory.Initialise<TestInstruction>();
            return instruction;
        }
    }
}