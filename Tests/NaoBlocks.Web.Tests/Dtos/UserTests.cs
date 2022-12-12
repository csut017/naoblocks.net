using System;
using Xunit;
using Data = NaoBlocks.Engine.Data;
using Transfer = NaoBlocks.Web.Dtos;

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
            var dto = Transfer.User.FromModel(entity, Transfer.DetailsType.Standard);
            Assert.Same(dto.Settings, entity.Settings);
        }

        [Fact]
        public void FromModelConvertsEntityWithParseDetails()
        {
            var now = DateTime.Now;
            var entity = Data.ItemImport.New(new Data.User
            {
                Name = "Moana",
                Role = Data.UserRole.Teacher,
                WhenAdded = now,
                Settings = new Data.UserSettings()
            }, message: "Some test details");
            var dto = Transfer.User.FromModel(entity, Transfer.DetailsType.Parse);
            Assert.Same("Moana", dto.Name);
            Assert.Same("Some test details", dto.Message);
        }
    }
}