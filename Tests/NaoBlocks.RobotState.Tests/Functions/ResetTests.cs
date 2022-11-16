using Moq;
using NaoBlocks.RobotState.Functions;

namespace NaoBlocks.RobotState.Tests.Functions
{
    public class ResetTests
    {
        [Fact]
        public void ConstructorSetsName()
        {
            var func = new Reset();
            Assert.Equal("reset", func.Name);
        }

        [Fact]
        public async Task ExecuteAsyncCallsReset()
        {
            var func = new Reset();
            var engine = new Mock<IEngine>();
            engine.Setup(e => e.ResetAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            var result = await func.ExecuteAsync(engine.Object, Common.EmptyNode);
            engine.Verify();
            Assert.True(result.WasSuccessful);
        }
    }
}