namespace NaoBlocks.Client.Terminal.Tests
{
    public class StandardConsoleTests
    {
        // [Fact]
        [Fact(Skip = "This test must be explicitly run as it interacts with the console")]
        public void AllTests()
        {
            // These tests should be in their own test functions, but there is some non-deterministic functionality happening when
            // we try to test using the Console :-(

            // Arrange
            var writer = new StringWriter();
            Console.ResetColor();
            Console.SetOut(writer);
            var colour = Console.ForegroundColor;
            var console = new StandardConsole();

            // Act
            console.WriteError("error");
            console.WriteMessage("message");

            // Assert
            var output = writer.ToString();
            Assert.Equal(
                "error\r\nmessage\r\n",
                output);
            Assert.Equal(colour, Console.ForegroundColor);
        }
    }
}