using NaoBlocks.Engine.Indices;
using Xunit;

namespace NaoBlocks.Engine.Tests.Indices
{
    public class RobotLogByMachineNameTests 
    {
        [Fact]
        public void ConstructorWorks()
        {
            var index = new RobotLogByMachineName();
            Assert.False(index.IsMapReduce);
        }
    }
}
