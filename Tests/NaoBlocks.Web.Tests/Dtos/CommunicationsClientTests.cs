using Transfer = NaoBlocks.Web.Dtos;
using Data = NaoBlocks.Engine.Data;
using Xunit;
using System;
using NaoBlocks.Web.Communications;
using System.Net.WebSockets;
using Moq;

namespace NaoBlocks.Web.Tests.Dtos
{
    public class CommunicationsClientTests
    {
        [Fact]
        public void FromModelConvertsUserEntity()
        {
            var entity = new WebSocketClientConnection(
                new Mock<WebSocket>().Object, 
                ClientConnectionType.User, 
                new Mock<IMessageProcessor>().Object, 
                new FakeLogger<WebSocketClientConnection>())
            {
                Id = 1,
                User = new Data.User { Name = "Mia" }
            };
            var dto = Transfer.CommunicationsClient.FromModel(entity);
            Assert.Equal("Mia", dto.User?.Name);
            Assert.Equal(1, dto.Id);
            Assert.Equal(ClientConnectionType.User, dto.Type);
            Assert.False(dto.IsClosing);
            Assert.Null(dto.Robot);
        }

        [Fact]
        public void FromModelConvertsRobotEntity()
        {
            var entity = new WebSocketClientConnection(
                new Mock<WebSocket>().Object,
                ClientConnectionType.Robot,
                new Mock<IMessageProcessor>().Object,
                new FakeLogger<WebSocketClientConnection>())
            {
                Id = 3,
                Robot = new Data.Robot { MachineName = "karetao" }
            };
            entity.Status.IsAvailable = true;
            var dto = Transfer.CommunicationsClient.FromModel(entity);
            Assert.Equal("karetao", dto.Robot?.MachineName);
            Assert.Equal(3, dto.Id);
            Assert.Equal(ClientConnectionType.Robot, dto.Type);
            Assert.False(dto.IsClosing);
            Assert.Null(dto.User);
        }
    }
}
