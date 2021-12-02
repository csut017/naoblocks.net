using Moq;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Web.Communications;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.Tests.Communications
{
    public class MessageProcessorTests
    {
        [Fact]
        public async Task ProcessAsyncHandlesMissingCommand()
        {
            // Arrange
            var hub = new Mock<IHub>();
            var logger = new FakeLogger<MessageProcessor>();
            var factory = new Mock<IEngineFactory>();
            var socket = new Mock<WebSocket>();
            var processor = new MessageProcessor(hub.Object, logger, factory.Object);
            var client = new ClientConnection(socket.Object, ClientConnectionType.Unknown, processor);

            // Act
            var msg = new ClientMessage(ClientMessageType.Unknown);
            await processor.ProcessAsync(client, msg);

            // Assert
            var msgs = client.RetrievePendingMessages();
            Assert.Equal(new[] { "Error" }, msgs.Select(m => m.Type.ToString()).ToArray());
            Assert.Equal(new[] { "Unable to find processor for Unknown" }, msgs.Select(m => m.Values["error"]).ToArray());
        }

        [Fact]
        public async Task ProcessAyncReturnsError()
        {
            // Arrange
            var hub = new Mock<IHub>();
            var logger = new FakeLogger<MessageProcessor>();
            var factory = new Mock<IEngineFactory>();
            var session = new Mock<IDatabaseSession>();
            factory.Setup(f => f.Initialise())
                .Throws(new Exception("error"));
            var socket = new Mock<WebSocket>();
            var processor = new MessageProcessor(hub.Object, logger, factory.Object);
            var client = new ClientConnection(socket.Object, ClientConnectionType.Unknown, processor);

            // Act
            var msg = new ClientMessage(ClientMessageType.ProgramStarted);
            await processor.ProcessAsync(client, msg);

            // Assert
            var msgs = client.RetrievePendingMessages();
            Assert.Equal(new[] { "Error" }, msgs.Select(m => m.Type.ToString()).ToArray());
            Assert.Equal(new[] { "Unable to process message: error" }, msgs.Select(m => m.Values["error"]).ToArray());
        }

        [Fact]
        public async Task ProcessAyncSavesSession()
        {
            // Arrange
            var hub = new Mock<IHub>();
            var logger = new FakeLogger<MessageProcessor>();
            var factory = new Mock<IEngineFactory>();
            var engine = new FakeEngine();
            var session = new Mock<IDatabaseSession>();
            factory.Setup(f => f.Initialise())
                .Returns((engine, session.Object));
            var socket = new Mock<WebSocket>();
            var processor = new MessageProcessor(hub.Object, logger, factory.Object);
            var client = new ClientConnection(socket.Object, ClientConnectionType.Unknown, processor);

            // Act
            var msg = new ClientMessage(ClientMessageType.ProgramStarted);
            await processor.ProcessAsync(client, msg);

            // Assert
            var msgs = client.RetrievePendingMessages();
            Assert.Empty(msgs.Select(m => m.Type.ToString()).ToArray());
            session.Verify(s => s.SaveChangesAsync());
            session.Verify(s => s.Dispose());
        }

        [Theory]
        [InlineData(ClientMessageType.ProgramStarted)]
        [InlineData(ClientMessageType.ProgramFinished)]
        [InlineData(ClientMessageType.ProgramStopped)]
        [InlineData(ClientMessageType.UnableToDownloadProgram)]
        public async Task BroadcastSendsToListenersAndLogs(ClientMessageType type)
        {
            // Arrange
            var (_, processor, client) = InitialiseTestProcessor();
            var listener = InitialiseListener(client);

            // Act
            var msg = new ClientMessage(type);
            await processor.ProcessAsync(client, msg);

            // Assert
            Assert.Equal(new[] { type }, listener.RetrievePendingMessages().Select(m => m.Type).ToArray());
            var log = await client.GetMessageLogAsync();
            Assert.Equal(new[] { type }, log.Select(m => m.Type).ToArray());
        }

        private static (FakeEngine, MessageProcessor, ClientConnection) InitialiseTestProcessor()
        {
            var hub = new Mock<IHub>();
            var logger = new FakeLogger<MessageProcessor>();
            var factory = new Mock<IEngineFactory>();
            var engine = new FakeEngine();
            var session = new Mock<IDatabaseSession>();
            factory.Setup(f => f.Initialise())
                .Returns((engine, session.Object));
            var socket = new Mock<WebSocket>();
            var processor = new MessageProcessor(hub.Object, logger, factory.Object);
            var client = new ClientConnection(socket.Object, ClientConnectionType.Unknown, processor);
            return (engine, processor, client);
        }

        private static ClientConnection InitialiseListener(ClientConnection client)
        {
            var hub = new Mock<IHub>();
            var logger = new FakeLogger<MessageProcessor>();
            var factory = new Mock<IEngineFactory>();
            var socket = new Mock<WebSocket>();
            var processor = new MessageProcessor(hub.Object, logger, factory.Object);
            var listener = new ClientConnection(socket.Object, ClientConnectionType.Unknown, processor);
            client.AddListener(listener);
            return listener;
        }
    }
}
