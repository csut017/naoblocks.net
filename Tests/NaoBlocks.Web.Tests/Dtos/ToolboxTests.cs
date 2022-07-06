using System;
using Xunit;
using Data = NaoBlocks.Engine.Data;
using Transfer = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Tests.Dtos
{
    public class ToolboxTests
    {
        [Fact]
        public void FromModelConvertsEntity()
        {
            var entity = new Data.Toolbox
            {
                Name = "karetao",
                IsDefault = true
            };
            var dto = Transfer.Toolbox.FromModel(entity);
            Assert.Equal("karetao", dto.Name);
            Assert.True(dto.IsDefault);
        }

        [Fact]
        public void FromModelConvertsEntityAndXml()
        {
            var entity = new Data.Toolbox
            {
                Name = "karetao",
                IsDefault = true
            };
            entity.Categories.Add(
                new Data.ToolboxCategory { Name = "testing" });
            var dto = Transfer.Toolbox.FromModel(entity, true);
            Assert.Equal(
                "<toolbox><category name=\"testing\" colour=\"0\" optional=\"no\" /></toolbox>",
                dto.Definition);
        }
    }
}