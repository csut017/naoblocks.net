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
        [InlineData(ClientMessageType.ProgramStarted, ClientMessageType.ProgramStarted)]
        [InlineData(ClientMessageType.ProgramFinished, ClientMessageType.ProgramFinished)]
        [InlineData(ClientMessageType.ProgramStopped, ClientMessageType.ProgramStopped)]
        [InlineData(ClientMessageType.ProgramDownloaded, ClientMessageType.ProgramTransferred)]
        [InlineData(ClientMessageType.UnableToDownloadProgram, ClientMessageType.UnableToDownloadProgram)]
        public async Task ProcessAsyncSendsToListenersAndLogs(ClientMessageType inputType, ClientMessageType outputType)
        {
            // Arrange
            var (_, processor, client) = InitialiseTestProcessor();
            var listener = InitialiseListener(client);

            // Act
            var msg = new ClientMessage(inputType);
            await processor.ProcessAsync(client, msg);

            // Assert
            Assert.Equal(new[] { outputType }, listener.RetrievePendingMessages().Select(m => m.Type).ToArray());
            var log = await client.GetMessageLogAsync();
            Assert.Equal(new[] { outputType }, log.Select(m => m.Type).ToArray());
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
            AddToRobotLog? command = null;
            engine.OnExecute = c =>
            {
                command = c as AddToRobotLog;
                return CommandResult.New(3);
            };
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
            Assert.Equal("14916", command?.Values.FirstOrDefault(nv => nv.Name == "programId")?.Value);
        }

        [Theory]
        [InlineData(ClientConnectionType.Robot, "Robot")]
        [InlineData(ClientConnectionType.User, "User")]
        [InlineData(ClientConnectionType.Unknown, "Unknown")]
        public async Task BroadcastSendsToMonitors(ClientConnectionType type, string expectedType)
        {
            // Arrange
            var hub = new Mock<IHub>();
            ClientMessage? receivedMessage = null;
            hub.Setup(h => h.SendToMonitors(It.IsAny<ClientMessage>()))
                .Callback((ClientMessage m) =>
                {
                    receivedMessage = m;
                })
                .Verifiable();
            var (_, processor, client) = InitialiseTestProcessor(type, hub.Object);

            // Act
            var msg = new ClientMessage(ClientMessageType.ProgramStarted)
            {
                ConversationId = 963
            };
            await processor.ProcessAsync(client, msg);

            // Assert
            hub.VerifyAll();
            Assert.Equal(expectedType, receivedMessage?.Values["SourceType"]);
        }

        [Fact]
        public async Task BroadcastSendsUserDetails()
        {
            // Arrange
            var hub = new Mock<IHub>();
            ClientMessage? receivedMessage = null;
            hub.Setup(h => h.SendToMonitors(It.IsAny<ClientMessage>()))
                .Callback((ClientMessage m) =>
                {
                    receivedMessage = m;
                })
                .Verifiable();
            var (_, processor, client) = InitialiseTestProcessor(ClientConnectionType.User, hub.Object);
            client.User = new Data.User { Name = "Mia" };

            // Act
            var msg = new ClientMessage(ClientMessageType.ProgramStarted)
            {
                ConversationId = 963
            };
            await processor.ProcessAsync(client, msg);

            // Assert
            hub.VerifyAll();
            Assert.Equal("User", receivedMessage?.Values["SourceType"]);
            Assert.Equal("Mia", receivedMessage?.Values["SourceName"]);
        }

        [Fact]
        public async Task BroadcastSendsRobotDetails()
        {
            // Arrange
            var hub = new Mock<IHub>();
            ClientMessage? receivedMessage = null;
            hub.Setup(h => h.SendToMonitors(It.IsAny<ClientMessage>()))
                .Callback((ClientMessage m) =>
                {
                    receivedMessage = m;
                })
                .Verifiable();
            var (_, processor, client) = InitialiseTestProcessor(ClientConnectionType.Robot, hub.Object);
            client.Robot = new Data.Robot { MachineName = "Mihīni" };

            // Act
            var msg = new ClientMessage(ClientMessageType.ProgramStarted)
            {
                ConversationId = 963
            };
            await processor.ProcessAsync(client, msg);

            // Assert
            hub.VerifyAll();
            Assert.Equal(
                new[] { "SourceClientId=>0", "SourceName=>Mihīni", "SourceType=>Robot" },
                receivedMessage?.Values.Select(value => $"{value.Key}=>{value.Value}").OrderBy(v => v).ToArray());
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

        [Fact]
        public async Task ProgramDownloadedCopiesProgramId()
        {
            // Arrange
            var hub = new Mock<IHub>();
            ClientMessage? receivedMessage = null;
            hub.Setup(h => h.SendToMonitors(It.IsAny<ClientMessage>()))
                .Callback((ClientMessage m) =>
                {
                    receivedMessage = m;
                })
                .Verifiable();
            var (_, processor, client) = InitialiseTestProcessor(ClientConnectionType.Robot, hub.Object);
            client.Robot = new Data.Robot { MachineName = "Mihīni" };
            client.RobotDetails = new RobotStatus { LastProgramId = 11235 };

            // Act
            var msg = new ClientMessage(ClientMessageType.ProgramDownloaded)
            {
                ConversationId = 963
            };
            await processor.ProcessAsync(client, msg);

            // Assert
            hub.VerifyAll();
            Assert.Equal(
                new[] { "ProgramId=>11235", "SourceClientId=>0", "SourceName=>Mihīni", "SourceType=>Robot" },
                receivedMessage?.Values.Select(value => $"{value.Key}=>{value.Value}").OrderBy(v => v).ToArray());
        }

        [Theory]
        [InlineData(ClientMessageType.StopProgram)]
        [InlineData(ClientMessageType.TransferProgram)]
        public async Task ProcessAsyncChecksAuthentication(ClientMessageType messageType)
        {
            // Arrange
            var (_, processor, client) = InitialiseTestProcessor();

            // Act
            var msg = new ClientMessage(messageType);
            await processor.ProcessAsync(client, msg);

            // Assert
            var msgs = client.RetrievePendingMessages();
            Assert.Equal(new[] { ClientMessageType.NotAuthenticated }, msgs.Select(m => m.Type).ToArray());
        }

        [Theory]
        [InlineData(ClientMessageType.StopProgram)]
        [InlineData(ClientMessageType.TransferProgram)]
        public async Task ProcessAsyncChecksConnectionIsForUser(ClientMessageType messageType)
        {
            // Arrange
            var (_, processor, client) = InitialiseTestProcessor();
            client.Robot = new Data.Robot();

            // Act
            var msg = new ClientMessage(messageType);
            await processor.ProcessAsync(client, msg);

            // Assert
            var msgs = client.RetrievePendingMessages();
            Assert.Equal(new[] { ClientMessageType.Forbidden }, msgs.Select(m => m.Type).ToArray());
        }

        [Theory]
        [InlineData(ClientMessageType.StopProgram, ClientMessageType.ProgramStopped)]
        [InlineData(ClientMessageType.TransferProgram)]
        public async Task ProcessAsyncValidatesRobotIsSet(ClientMessageType messageType, params ClientMessageType[] extraMessages)
        {
            // Arrange
            var (_, processor, client) = InitialiseTestProcessor();
            client.User = new Data.User();

            // Act
            var msg = new ClientMessage(messageType);
            await processor.ProcessAsync(client, msg);

            // Assert
            var msgs = client.RetrievePendingMessages();
            var expectedMessages = new ClientMessageType[extraMessages.Length + 1];
            expectedMessages[0] = ClientMessageType.Error;
            Array.Copy(extraMessages, 0, expectedMessages, 1, extraMessages.Length);
            Assert.Equal(expectedMessages, msgs.Select(m => m.Type).ToArray());
            Assert.Equal(
                new[] { "Robot is missing" },
                msgs.Where(m => m.Values.ContainsKey("error")).Select(m => m.Values["error"]).ToArray());
        }

        [Theory]
        [InlineData(ClientMessageType.StopProgram, ClientMessageType.ProgramStopped)]
        [InlineData(ClientMessageType.TransferProgram)]
        public async Task ProcessAsyncValidatesRobotIdIsValid(ClientMessageType messageType, params ClientMessageType[] extraMessages)
        {
            // Arrange
            var (_, processor, client) = InitialiseTestProcessor();
            client.User = new Data.User();

            // Act
            var msg = new ClientMessage(messageType, new { robot = "alpha" });
            await processor.ProcessAsync(client, msg);

            // Assert
            var msgs = client.RetrievePendingMessages();
            var expectedMessages = new ClientMessageType[extraMessages.Length + 1];
            expectedMessages[0] = ClientMessageType.Error;
            Array.Copy(extraMessages, 0, expectedMessages, 1, extraMessages.Length);
            Assert.Equal(expectedMessages, msgs.Select(m => m.Type).ToArray());
            Assert.Equal(
                new[] { "Robot id is invalid" },
                msgs.Where(m => m.Values.ContainsKey("error")).Select(m => m.Values["error"]).ToArray());
        }

        [Theory]
        [InlineData(ClientMessageType.StopProgram, ClientMessageType.ProgramStopped)]
        [InlineData(ClientMessageType.TransferProgram)]
        public async Task ProcessAsyncValidatesRobotIsConnected(ClientMessageType messageType, params ClientMessageType[] extraMessages)
        {
            // Arrange
            var (_, processor, client) = InitialiseTestProcessor();
            client.User = new Data.User();

            // Act
            var msg = new ClientMessage(messageType, new { robot = "14916" });
            await processor.ProcessAsync(client, msg);

            // Assert
            var msgs = client.RetrievePendingMessages();
            var expectedMessages = new ClientMessageType[extraMessages.Length + 1];
            expectedMessages[0] = ClientMessageType.Error;
            Array.Copy(extraMessages, 0, expectedMessages, 1, extraMessages.Length);
            Assert.Equal(expectedMessages, msgs.Select(m => m.Type).ToArray());
            Assert.Equal(
                new[] { "Robot is no longer connected" },
                msgs.Where(m => m.Values.ContainsKey("error")).Select(m => m.Values["error"]).ToArray());
        }

        [Theory]
        [InlineData(ClientMessageType.StopProgram, ClientMessageType.StopProgram, "")]
        [InlineData(ClientMessageType.TransferProgram, ClientMessageType.DownloadProgram, "program=14916")]
        public async Task ProcessAsyncChecksRobotIsSentMessage(ClientMessageType inputMessage, ClientMessageType outputMessage, string parameters)
        {
            // Arrange
            var robotClient = InitialiseClient();
            var hub = new Mock<IHub>();
            hub.Setup(h => h.GetClient(14916)).Returns(robotClient);
            var (_, processor, client) = InitialiseTestProcessor(hub: hub.Object);
            client.User = new Data.User();

            // Act
            var msg = new ClientMessage(inputMessage, new { robot = "14916" });
            InitialiseMessageParameters(parameters, msg);
            await processor.ProcessAsync(client, msg);

            // Assert
            var msgs = robotClient.RetrievePendingMessages();
            Assert.Equal(new[] { outputMessage }, msgs.Select(m => m.Type).ToArray());
        }

        [Theory]
        [InlineData(ClientMessageType.StopProgram, "")]
        [InlineData(ClientMessageType.TransferProgram, "program=14916")]
        public async Task ProcessAsyncWarnsIfRobotNotSet(ClientMessageType inputMessage, string parameters)
        {
            // Arrange
            var robotClient = InitialiseClient();
            var hub = new Mock<IHub>();
            hub.Setup(h => h.GetClient(14916)).Returns(robotClient);
            var (_, processor, client) = InitialiseTestProcessor(hub: hub.Object);
            client.User = new Data.User();

            // Act
            var msg = new ClientMessage(inputMessage, new { robot = "14916" });
            InitialiseMessageParameters(parameters, msg);
            await processor.ProcessAsync(client, msg);

            // Assert
            var messages = RetrieveLogMessages(processor);
            Assert.Equal(new[] {
                $"INFORMATION: Processing message type {inputMessage}",
                "WARNING: Unable to add to log: robot is missing"
            }, messages);
        }

        [Theory]
        [InlineData(ClientMessageType.StopProgram, "Program stopping", "")]
        [InlineData(ClientMessageType.TransferProgram, "Program transferring", "program=14916")]
        public async Task ProcessAsyncLogsMessage(ClientMessageType inputMessage, string logMessage, string parameters)
        {
            // Arrange
            var robotClient = InitialiseClient();
            robotClient.Robot = new Data.Robot { MachineName = "Mihīni" };
            var hub = new Mock<IHub>();
            hub.Setup(h => h.GetClient(14916)).Returns(robotClient);
            var (engine, processor, client) = InitialiseTestProcessor(hub: hub.Object);
            AddToRobotLog? command = null;
            engine.OnExecute = c =>
            {
                command = c as AddToRobotLog;
                return CommandResult.New(3);
            };
            engine.ExpectCommand<AddToRobotLog>();
            client.User = new Data.User();

            // Act
            var msg = new ClientMessage(inputMessage, new { robot = "14916" })
            {
                ConversationId = 36912
            };
            InitialiseMessageParameters(parameters, msg);
            await processor.ProcessAsync(client, msg);

            // Assert
            engine.Verify();
            Assert.Equal("Mihīni", command?.MachineName);
            Assert.Equal(logMessage, command?.Description);
        }

        private static string[] RetrieveLogMessages(MessageProcessor processor)
        {
            var logger = (FakeLogger<MessageProcessor>)processor.Logger;
            return logger.Messages.ToArray();
        }

        private static (FakeEngine, MessageProcessor, ClientConnection) InitialiseTestProcessor(ClientConnectionType type = ClientConnectionType.Unknown, IHub? hub = null)
        {
            hub ??= new Mock<IHub>().Object;
            var logger = new FakeLogger<MessageProcessor>();
            var factory = new Mock<IEngineFactory>();
            var engine = new FakeEngine();
            var session = new Mock<IDatabaseSession>();
            factory.Setup(f => f.Initialise())
                .Returns((engine, session.Object));
            var socket = new Mock<WebSocket>();
            var processor = new MessageProcessor(hub, logger, factory.Object);
            var client = new ClientConnection(socket.Object, type, processor);
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

        private static ClientConnection InitialiseClient()
        {
            var hub = new Mock<IHub>();
            var logger = new FakeLogger<MessageProcessor>();
            var factory = new Mock<IEngineFactory>();
            var socket = new Mock<WebSocket>();
            var processor = new MessageProcessor(hub.Object, logger, factory.Object);
            var client = new ClientConnection(socket.Object, ClientConnectionType.Unknown, processor);
            return client;
        }

        private static void InitialiseMessageParameters(string parameters, ClientMessage msg)
        {
            if (!string.IsNullOrEmpty(parameters))
            {
                foreach (var parameter in parameters.Split(",", StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = parameter.Split('=');
                    msg.Values.Add(parts[0], parts[1]);
                }
            }
        }
    }
}
