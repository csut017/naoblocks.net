using Transfer = NaoBlocks.Web.Dtos;
using Data = NaoBlocks.Engine.Data;
using Xunit;
using System;
using System.Linq;

namespace NaoBlocks.Web.Tests.Dtos
{
    public class SnapshotTests
    {
        [Fact]
        public void FromModelConvertsEntity()
        {
            var now = DateTime.Now;
            var entity = new Data.Snapshot
            {
                Source = "hōtaka",
                State = "tika",
                User = new Data.User { Name = "Mia" },
                WhenAdded = now
            };
            var dto = Transfer.Snapshot.FromModel(entity);
            Assert.Equal("hōtaka", dto.Source);
            Assert.Equal("tika", dto.State);
            Assert.Equal("Mia", dto.User);
            Assert.Equal(entity.WhenAdded, dto.WhenAdded);
            Assert.Null(dto.Values);
        }

        [Fact]
        public void FromModelConvertsEntityWithValues()
        {
            var entity = new Data.Snapshot
            {
                Source = "hōtaka",
                State = "tika",
                WhenAdded = DateTime.Now
            };
            entity.Values.Add(new Data.NamedValue { Name = "tahi", Value = "rua" });
            var dto = Transfer.Snapshot.FromModel(entity);
            Assert.Null(dto.User);
            Assert.NotNull(dto.Values);
            Assert.Equal(new[] { "tahi=rua" }, dto.Values!.Select(nv => $"{nv.Name}={nv.Value}").ToArray());
        }
    }
}
