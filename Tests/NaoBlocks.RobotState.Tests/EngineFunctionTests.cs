namespace NaoBlocks.RobotState.Tests
{
    public class EngineFunctionTests
    {
        [Fact]
        public void ConstructorSetsName()
        {
            var func = new EngineFunction("testing", eng => Task.FromResult(new EngineFunctionResult()));
            Assert.Equal("testing", func?.Name);
        }

        [Fact]
        public async Task ExecuteAsyncCallsFunction()
        {
            // Arrange
            var wasCalled = false;
            var func = new EngineFunction("testing", eng =>
            {
                wasCalled = true;
                return Task.FromResult(new EngineFunctionResult());
            });

            // Act
            var result = await func.ExecuteAsync(new Engine());

            // Assert
            Assert.True(wasCalled);
        }
    }
}