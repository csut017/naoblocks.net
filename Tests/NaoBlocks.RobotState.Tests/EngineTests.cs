namespace NaoBlocks.RobotState.Tests
{
    public class EngineTests
    {
        [Fact]
        public async Task InitialiseHandlesInvalidProgram()
        {
            // Arrange
            var engine = new Engine();

            // Act
            var error = await Assert.ThrowsAsync<EngineException>(async () => await engine.InitialiseAsync("go("));

            // Assert
            Assert.Equal(
                "Unable to parse program code:\r\n* Unable to parse function arg {:EOF} [0:3]",
                error.Message);
        }

        [Fact]
        public async Task InitialiseSetsInitialState()
        {
            // Arrange
            var engine = new Engine();

            // Act
            await engine.InitialiseAsync("go()");

            // Assert
            var program = engine.ToString();
            Assert.Equal(
                $"Program:0=>Function:go{Environment.NewLine}Variables:{{}}",
                program);
        }
    }
}