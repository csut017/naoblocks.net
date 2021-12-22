using Transfer = NaoBlocks.Web.Dtos;
using Data = NaoBlocks.Engine.Data;
using Xunit;
using System;

namespace NaoBlocks.Web.Tests.Dtos
{
    public class RobotTests
    {
        [Fact]
        public void FromModelConvertsEntity()
        {
            var now = DateTime.Now;
            var entity = new Data.Robot
            {
                FriendlyName = "Moana",
                MachineName = "karetao",
                Type = new Data.RobotType { Name = "mihini" },
                WhenAdded = now
            };
            var dto = Transfer.Robot.FromModel(entity);
            Assert.Equal("Moana", dto.FriendlyName);
            Assert.Equal("karetao", dto.MachineName);
            Assert.Equal("mihini", dto.Type);
            Assert.False(dto.IsInitialised);
        }

        [Fact]
        public void FromModelHandlesMissingType()
        {
            var now = DateTime.Now;
            var entity = new Data.Robot
            {
                FriendlyName = "Moana",
                MachineName = "karetao",
                IsInitialised = true,
                WhenAdded = now
            };
            var dto = Transfer.Robot.FromModel(entity);
            Assert.Equal("Moana", dto.FriendlyName);
            Assert.Equal("karetao", dto.MachineName);
            Assert.Null(dto.Type);
            Assert.True(dto.IsInitialised);
        }
    }
}
