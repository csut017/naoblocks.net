using Transfer = NaoBlocks.Web.Dtos;
using Data = NaoBlocks.Engine.Data;
using Xunit;
using System;

namespace NaoBlocks.Web.Tests.Dtos
{
    public class TeacherTests
    {
        [Fact]
        public void FromModelConvertsEntity()
        {
            var now = DateTime.Now;
            var entity = new Data.User
            {
                Name = "Moana",
                Role = Data.UserRole.Teacher,
                WhenAdded = now
            };
            var dto = Transfer.Teacher.FromModel(entity);
            Assert.Equal(entity.Name, dto.Name);
            Assert.Null(dto.Role);
            Assert.Null(dto.WhenAdded);
        }
    }
}
