using NaoBlocks.Client.Terminal.Instructions;
using NaoBlocks.Common;
using Newtonsoft.Json;
using System.Net;
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
            var client = new FakeHttpMessageHandler();
            client.AddOutgoing(
                HttpStatusCode.OK,
                JsonConvert.SerializeObject(new VersionInformation
                {
                    Version = "1.2.3.4"
                }));
            instruction.Factory!.OnInitialiseConnection = connection => connection.InitialiseHttpClient = () => new HttpClient(client);

            // Act
            instruction.Validate(console, new[] { "help", "http://server" });
            var result = await instruction.RunAsync(console);

            // Assert
            Assert.Equal(0, result);
            Assert.Equal(
                new[] { "INFO: The current version is 1.2.3.4" },
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

        [Theory]
        [InlineData("unknown", false)]
        [InlineData("ftp://server", false)]
        [InlineData("http://server", true)]
        [InlineData("https://server", true)]
        public void ValidateChecksServerIsSet(string serverAddress, bool isValid)
        {
            // Arrange
            var instruction = InitialiseInstruction();
            var console = new TestConsole();

            // Act
            var result = instruction.Validate(console, new[] { "help", serverAddress });

            // Assert
            if (isValid)
            {
                Assert.True(result);
            }
            else
            {
                Assert.False(result);
                Assert.Equal("ERROR: Invalid server address - must start with https:// or http://:INFO::INFO: Usage is NaoBlocks version <SERVER> [options]", string.Join(":", console.Output).Trim());
            }
        }

        [Theory]
        [InlineData(false, invalidNumberOfArguments, "version")]
        [InlineData(true, "", "version", "http://test")]
        [InlineData(false, invalidNumberOfArguments, "help", "test", "three")]
        [InlineData(false, invalidNumberOfArguments, "help", "test", "four")]
        public void ValidateTakesOneArgument(bool allowed, string output, params string[] args)
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