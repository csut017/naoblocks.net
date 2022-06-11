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
    }
}