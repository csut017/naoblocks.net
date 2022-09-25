namespace NaoBlocks.RobotState.Tests
{
    public class EngineFunctionResultTests
    {
        [Fact]
        public void ConstructorHandlesException()
        {
            var result = new EngineFunctionResult(new Exception("testing"));
            Assert.False(result.WasSuccessful);
            Assert.Equal("testing", result.ErrorMessage);
        }

        [Fact]
        public void ConstructorSetsProperties()
        {
            var result = new EngineFunctionResult(true);
            Assert.True(result.WasSuccessful);
        }
    }
}