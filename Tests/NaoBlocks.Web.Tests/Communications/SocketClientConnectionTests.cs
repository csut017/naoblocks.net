using Microsoft.Extensions.Logging;
using Moq;
using NaoBlocks.Common;
using NaoBlocks.Communications;
using NaoBlocks.Web.Communications;
using System;
using Xunit;

namespace NaoBlocks.Web.Tests.Communications
{
    public class SocketClientConnectionTests
    {
        [Fact]
        public void ClosesRaisesEvent()
        {
            // Arrange
            var client = new Mock<IClient>();
            var processor = new Mock<IMessageProcessor>();
            var logger = new Mock<ILogger<SocketClientConnection>>();
            var connection = new SocketClientConnection(client.Object, ClientConnectionType.Robot, processor.Object, logger.Object);
            var eventRaised = false;
            connection.Closed += (o, e) => eventRaised = true;

            // Act
            connection.Close();

            // Assert
            Assert.True(eventRaised, "Closed event not raised");
        }

        [Fact]
        public void DisposeRaisesEvent()
        {
            // Arrange
            var client = new Mock<IClient>();
            var processor = new Mock<IMessageProcessor>();
            var logger = new Mock<ILogger<SocketClientConnection>>();
            var connection = new SocketClientConnection(client.Object, ClientConnectionType.Robot, processor.Object, logger.Object);
            var eventRaised = false;
            connection.Closed += (o, e) => eventRaised = true;

            // Act
            connection.Dispose();

            // Assert
            Assert.True(eventRaised, "Closed event not raised");
        }

        [Fact]
        public void ReceiveMessageCallsProcessor()
        {
            // Arrange
            var client = new Mock<IClient>();
            var processor = new Mock<IMessageProcessor>();
            var logger = new Mock<ILogger<SocketClientConnection>>();
            using var connection = new SocketClientConnection(client.Object, ClientConnectionType.Robot, processor.Object, logger.Object);

            // Act
            var message = new ReceivedMessage(client.Object, ClientMessageType.Unknown);
            client.Raise(c => c.MessageReceived += null, client, message);

            // Assert
            processor.Verify(p => p.ProcessAsync(It.IsAny<IClientConnection>(), message));
        }

        [Fact]
        public void SendMessageCallsClient()
        {
            // Arrange
            var client = new Mock<IClient>();
            var processor = new Mock<IMessageProcessor>();
            var logger = new Mock<ILogger<SocketClientConnection>>();
            using var connection = new SocketClientConnection(client.Object, ClientConnectionType.Robot, processor.Object, logger.Object);

            // Act
            connection.SendMessage(new ClientMessage());

            // Assert
            client.Verify(c => c.SendMessageAsync(It.IsAny<ClientMessage>(), It.IsAny<TimeSpan>()));
        }
    }
}