using System;
using System.Linq;
using Xunit;
using Data = NaoBlocks.Engine.Data;
using Transfer = NaoBlocks.Web.Dtos;

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

        [Fact]
        public void FromModelIncludesFullDetails()
        {
            var now = DateTime.Now;
            var entity = Data.ItemImport.New(new Data.Robot
            {
                FriendlyName = "Moana",
                MachineName = "karetao",
                WhenAdded = now,
            }, message: "This is a test message");
            entity.Item!.CustomValues.Add(Data.NamedValue.New("One", "Tahi"));
            var dto = Transfer.Robot.FromModel(entity, Transfer.DetailsType.Parse | Transfer.DetailsType.Standard);
            Assert.Equal(
                "This is a test message",
                dto?.Parse?.Message);
            Assert.Equal(
                new[] { "One->Tahi" },
                dto?.Values?.Select(nv => $"{nv.Name}->{nv.Value}").ToArray());
        }

        [Fact]
        public void FromModelIncludesParseMessage()
        {
            var now = DateTime.Now;
            var entity = Data.ItemImport.New(new Data.Robot
            {
                FriendlyName = "Moana",
                MachineName = "karetao",
                WhenAdded = now,
            }, message: "This is a test message");
            var dto = Transfer.Robot.FromModel(entity, Transfer.DetailsType.Parse);
            Assert.Equal(
                "This is a test message",
                dto?.Parse?.Message);
        }

        [Fact]
        public void FromModelIncludesValues()
        {
            var now = DateTime.Now;
            var entity = new Data.Robot
            {
                FriendlyName = "Moana",
                MachineName = "karetao",
                WhenAdded = now
            };
            entity.CustomValues.Add(Data.NamedValue.New("One", "Tahi"));
            var dto = Transfer.Robot.FromModel(entity, Transfer.DetailsType.Standard);
            Assert.Equal(
                new[] { "One->Tahi" },
                dto?.Values?.Select(nv => $"{nv.Name}->{nv.Value}").ToArray());
        }
    }
}