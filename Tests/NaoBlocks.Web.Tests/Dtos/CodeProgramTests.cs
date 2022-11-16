using Transfer = NaoBlocks.Web.Dtos;
using Data = NaoBlocks.Engine.Data;
using Xunit;
using System;

namespace NaoBlocks.Web.Tests.Dtos
{
    public class CodeProgramTests
    {
        [Fact]
        public void FromModelConvertsEntity()
        {
            var now = DateTime.Now;
            var entity = new Data.CodeProgram
            {
                Name = "Moana",
                Number = 14916,
                WhenAdded = now
            };
            var dto = Transfer.CodeProgram.FromModel(entity);
            Assert.Equal("Moana", dto?.Name);
            Assert.Equal(14916, dto?.Id);
            Assert.Equal(now, dto?.WhenAdded);
            Assert.Null(dto?.Code);
        }

        [Fact]
        public void FromModelConvertsEntityWithDetails()
        {
            var now = DateTime.Now;
            var entity = new Data.CodeProgram
            {
                Name = "Moana",
                Number = 14916,
                WhenAdded = now,
                Code = "go()"
            };
            var dto = Transfer.CodeProgram.FromModel(entity, true);
            Assert.Equal("Moana", dto?.Name);
            Assert.Equal(14916, dto?.Id);
            Assert.Equal(now, dto?.WhenAdded);
            Assert.Equal("go()", dto?.Code);
        }
    }
}
