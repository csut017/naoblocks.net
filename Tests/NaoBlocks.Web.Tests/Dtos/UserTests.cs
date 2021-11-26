using Transfer = NaoBlocks.Web.Dtos;
using Data = NaoBlocks.Engine.Data;
using Xunit;
using System;

namespace NaoBlocks.Web.Tests.Dtos
{
    public class UserTests
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
            var dto = Transfer.User.FromModel(entity);
            Assert.Equal(dto.Name, entity.Name);
            Assert.Equal(dto.Role, entity.Role.ToString());
            Assert.Equal(dto.WhenAdded, entity.WhenAdded);
        }

        [Fact]
        public void FromModelConvertsEntityWithDetails()
        {
            var now = DateTime.Now;
            var entity = new Data.User
            {
                Name = "Moana",
                Role = Data.UserRole.Teacher,
                WhenAdded = now,
                Settings = new Data.UserSettings()
            };
            var dto = Transfer.User.FromModel(entity, true);
            Assert.Same(dto.Settings, entity.Settings);
        }
    }
}
