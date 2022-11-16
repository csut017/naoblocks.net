using Moq;
using NaoBlocks.RobotState.Functions;

namespace NaoBlocks.RobotState.Tests.Functions
{
    public class GoTests
    {
        [Fact]
        public void ConstructorSetsName()
        {
            var func = new Go();
            Assert.Equal("go", func.Name);
        }

        [Fact]
        public async Task ExecuteAsyncFindsStartAndCallsIt()
        {
            var func = new Go();
            var startFunc = new TestFunction("start");
            var engine = new Mock<IEngine>();
            engine.Setup(e => e.FindFunction("start"))
                .Returns(startFunc);

            var result = await func.ExecuteAsync(engine.Object, Common.EmptyNode);
            Assert.True(startFunc.WasCalled);
        }

        [Fact]
        public async Task ExecuteAsyncHandlesMissingStart()
        {
            var func = new Go();
            var engine = new Mock<IEngine>();
            engine.Setup(e => e.FindFunction("start"))
                .Returns((EngineFunction)null);

            var result = await func.ExecuteAsync(engine.Object, Common.EmptyNode);
            Assert.False(result.WasSuccessful);
            Assert.Equal("start has not been defined, cannot go", result.ErrorMessage);
        }
    }
}