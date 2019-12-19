using NaoBlocks.Core.Models;
using Xunit;

using Data = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Tests.Dtos
{
    public class RobotTests
    {
        [Fact]
        public void FromModelHandlesNull()
        {
            var dto = Data.Robot.FromModel(null);
            Assert.Null(dto);
        }

        [Fact]
        public void FromModelSetsProperties()
        {
            var robot = new Robot
            {
                FriendlyName = "A nice helpful robot",
                MachineName = "GoodBot"
            };
            var dto = Data.Robot.FromModel(robot);
            Assert.Equal(robot.FriendlyName, dto.FriendlyName);
            Assert.Equal(robot.MachineName, dto.MachineName);
        }
    }
}