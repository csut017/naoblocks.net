namespace NaoBlocks.Client.Terminal.Tests
{
    public class AppTests
    {
        [Fact]
        public void AppDisposesWithoutAnyErrors()
        {
            // Arrange
            using var app = new App();

            // This will disable any unused reference warnings
            Console.WriteLine(app.ToString());
        }

        [Fact]
        public async Task RunAsyncHandlesMissingInstruction()
        {
            // Arrange
            using var app = new App(typeof(TestInstruction));
            var console = new TestConsole();

            // Act
            var result = await app.RunAsync(console, new[] { "missing" });

            // Assert
            Assert.Equal(
                new[] { "ERROR: Unknown command 'missing'" },
                console.Output);
        }

        [Fact]
        public async Task RunAsyncHandlesNoInstruction()
        {
            // Arrange
            using var app = new App();
            var console = new TestConsole();

            // Act
            var result = await app.RunAsync(console, Array.Empty<string>());

            // Assert
            Assert.Contains(
                "INFO: \thelp: Displays the help information",
                console.Output);
        }

        [Fact]
        public async Task RunAsyncHandlesValidationFailure()
        {
            // Arrange
            var instruction = new TestInstruction
            {
                OnRunAsync = () => 2
            };
            using var app = new App(_ => instruction);
            var console = new TestConsole();

            // Act
            var result = await app.RunAsync(console, new[] { "test" });

            // Assert
            Assert.True(instruction.ValidateCalled);
            Assert.True(instruction.RunAsyncCalled);
            Assert.Equal(2, result);
        }

        [Fact]
        public async Task RunAsyncRunsInstruction()
        {
            // Arrange
            var instruction = new TestInstruction
            {
                OnValidate = _ => false
            };
            using var app = new App(_ => instruction);
            var console = new TestConsole();

            // Act
            var result = await app.RunAsync(console, new[] { "test" });

            // Assert
            Assert.True(instruction.ValidateCalled);
            Assert.False(instruction.RunAsyncCalled);
            Assert.Equal(-1, result);
        }
    }
}