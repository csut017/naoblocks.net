using Transfer = NaoBlocks.Web.Dtos;
using Data = NaoBlocks.Engine.Data;
using Xunit;
using System;
using System.Linq;
using NaoBlocks.Common;

namespace NaoBlocks.Web.Tests.Dtos
{
    public class RobotLogLineTests
    {
        [Fact]
        public void FromModelConvertsEntity()
        {
            var now = DateTime.Now;
            var entity = new Data.RobotLogLine
            {
                Description = "Kia ora",
                SourceMessageType = ClientMessageType.Authenticated,
                WhenAdded = now
            };
            var dto = Transfer.RobotLogLine.FromModel(entity);
            Assert.Equal("Kia ora", dto.Description);
            Assert.Equal(ClientMessageType.Authenticated, dto.SourceMessageType);
            Assert.Equal(now, dto.WhenAdded);
            Assert.Null(dto.Values);
        }

        [Fact]
        public void FromModelConvertsEntityWithValues()
        {
            var entity = new Data.RobotLogLine
            {
                Description = "Kia ora",
                SourceMessageType = ClientMessageType.Authenticated,
                WhenAdded = DateTime.Now
            };
            entity.Values.Add(new Data.NamedValue { Name = "tahi", Value = "rua" });
            entity.Values.Add(new Data.NamedValue { Name = "toru" });
            var dto = Transfer.RobotLogLine.FromModel(entity);
            Assert.NotNull(dto.Values);
            Assert.Equal(
                new[] { "tahi=rua", "toru=" }, 
                dto.Values!
                    .Select(nv => $"{nv.Key}={nv.Value}")
                    .OrderBy(v => v)
                    .ToArray());
        }
    }
}
