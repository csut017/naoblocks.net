using Transfer = NaoBlocks.Web.Dtos;
using Data = NaoBlocks.Engine.Data;
using Xunit;
using System;
using System.Linq;

namespace NaoBlocks.Web.Tests.Dtos
{
    public class RobotTypeTests
    {
        [Fact]
        public void FromModelConvertsEntity()
        {
            var now = DateTime.Now;
            var entity = new Data.RobotType
            {
                Name = "karetao",
                IsDefault = true,
                WhenAdded = now
            };
            var dto = Transfer.RobotType.FromModel(entity);
            Assert.Equal("karetao", dto.Name);
            Assert.True(dto.IsDefault);
        }

        [Fact]
        public void FromModelConvertsEntityAndToolbox()
        {
            var now = DateTime.Now;
            var entity = new Data.RobotType
            {
                Name = "karetao",
                IsDefault = true,
                WhenAdded = now
            };
            entity.Toolboxes.Add(new Data.Toolbox { Name = "blocks" });
            var dto = Transfer.RobotType.FromModel(entity, true);
            Assert.Equal(
                new[] { "blocks" },
                dto.Toolboxes?.ToArray());
        }
    }
}