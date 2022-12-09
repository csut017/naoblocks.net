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
        public void FromModelConvertsEntityAndRawXml()
        {
            var entity = new Data.Toolbox
            {
                Name = "karetao",
                IsDefault = true,
                RawXml = "<test/>"
            };
            var dto = Transfer.Toolbox.FromModel(entity, Transfer.DetailsType.Parse);
            Assert.Equal(
                "<test/>",
                dto.Definition);
        }

        [Fact]
        public void FromModelConvertsEntityAndXml()
        {
            var entity = new Data.Toolbox
            {
                Name = "karetao",
                IsDefault = true,
                UseEvents = true
            };
            entity.Categories.Add(
                new Data.ToolboxCategory { Name = "testing" });
            var dto = Transfer.Toolbox.FromModel(entity, Transfer.DetailsType.Standard);
            Assert.Equal(
                "<toolbox useEvents=\"yes\"><category name=\"testing\" colour=\"0\" optional=\"no\" /></toolbox>",
                dto.Definition);
        }

        [Fact]
        public void FromModelConvertsEntityAndXmlUsingFormat()
        {
            var entity = new Data.Toolbox
            {
                Name = "karetao",
                IsDefault = true
            };
            entity.Categories.Add(
                new Data.ToolboxCategory { Name = "testing" });
            var dto = Transfer.Toolbox.FromModel(entity, Transfer.DetailsType.Standard, "toolbox");
            Assert.Equal(
                "<toolbox useEvents=\"no\"><category name=\"testing\" colour=\"0\" optional=\"no\" /></toolbox>",
                dto.Definition);
        }

        [Fact]
        public void FromModelConvertsEntityIgnoresRawXmlIfUnset()
        {
            var entity = new Data.Toolbox
            {
                Name = "karetao",
                IsDefault = true
            };
            entity.Categories.Add(
                new Data.ToolboxCategory { Name = "testing" });
            var dto = Transfer.Toolbox.FromModel(entity, Transfer.DetailsType.Standard | Transfer.DetailsType.Parse);
            Assert.Equal(
                "<toolbox useEvents=\"no\"><category name=\"testing\" colour=\"0\" optional=\"no\" /></toolbox>",
                dto.Definition);
        }

        [Fact]
        public void FromModelConvertsEntityUsesRawXmlIfSet()
        {
            var entity = new Data.Toolbox
            {
                Name = "karetao",
                IsDefault = true,
                RawXml = "<test/>"
            };
            entity.Categories.Add(
                new Data.ToolboxCategory { Name = "testing" });
            var dto = Transfer.Toolbox.FromModel(entity, Transfer.DetailsType.Standard | Transfer.DetailsType.Parse);
            Assert.Equal(
                "<test/>",
                dto.Definition);
        }
    }
}