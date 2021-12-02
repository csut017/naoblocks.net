using Moq;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Web.Communications;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Xunit;

using Data = NaoBlocks.Engine.Data;

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

        [Fact]
        public async Task BroadcastCopiesValues()
        {
            // Arrange
            var (_, processor, client) = InitialiseTestProcessor();
            var listener = InitialiseListener(client);

            // Act
            var msg = new ClientMessage(ClientMessageType.UnableToDownloadProgram, new { programId = 14916 });
            await processor.ProcessAsync(client, msg);

            // Assert
            var output = listener.RetrievePendingMessages().First();
            Assert.Equal("14916", output.Values["programId"]);
        }

        [Fact]
        public async Task BroadcastLogsToRobotFailsWithoutAConversation()
        {
            // Arrange
            var (_, processor, client) = InitialiseTestProcessor();
            client.Robot = new Data.Robot();

            // Act
            var msg = new ClientMessage(ClientMessageType.ProgramStarted);
            await processor.ProcessAsync(client, msg);

            // Assert
            var messages = RetrieveLogMessages(processor);
            Assert.Equal(new[] {
                "INFORMATION: Processing message type ProgramStarted",
                "WARNING: Broadcast error: Adding to a robot log requires a conversation"
            }, messages);
        }

        [Fact]
        public async Task BroadcastLogsToRobotCallsCommand()
        {
            // Arrange
            var (engine, processor, client) = InitialiseTestProcessor();
            engine.ExpectCommand<AddToRobotLog>();
            client.Robot = new Data.Robot();

            // Act
            var msg = new ClientMessage(ClientMessageType.UnableToDownloadProgram, new { programId = 14916 })
            {
                ConversationId = 963
            };
            await processor.ProcessAsync(client, msg);

            // Assert
            engine.Verify();
        }

        [Fact]
        public async Task BroadcastLogsToRobotHandlesCommandFailure()
        {
            // Arrange
            var (engine, processor, client) = InitialiseTestProcessor();
            engine.OnExecute = c => new CommandResult(1, "Command error");
            client.Robot = new Data.Robot();

            // Act
            var msg = new ClientMessage(ClientMessageType.ProgramStarted)
            {
                ConversationId = 963
            };
            await processor.ProcessAsync(client, msg);

            // Assert
            var messages = RetrieveLogMessages(processor);
            Assert.Equal(new[] {
                "INFORMATION: Processing message type ProgramStarted",
                "WARNING: Broadcast error: Command error"
            }, messages);
        }

        private static string[] RetrieveLogMessages(MessageProcessor processor)
        {
            var logger = (FakeLogger<MessageProcessor>)processor.Logger;
            return logger.Messages.ToArray();
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
