using Microsoft.IdentityModel.Tokens;
using Moq;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Communications;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

using Data = NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.Tests.Communications
{
    public class MessageProcessorTests
    {
        private const string sessionId = "sessions/1";

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
            Assert.Equal(new[] { ClientMessageType.Error }, msgs.Select(m => m.Type).ToArray());
            Assert.Equal(new[] { "Unable to find processor for Unknown" }, RetrieveErrorMessages(msgs));
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
            Assert.Equal(new[] { ClientMessageType.Error }, msgs.Select(m => m.Type).ToArray());
            Assert.Equal(new[] { "Unable to process message: error" }, RetrieveErrorMessages(msgs));
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
            Assert.Empty(msgs.Select(m => m.Type).ToArray());
            session.Verify(s => s.SaveChangesAsync());
            session.Verify(s => s.Dispose());
        }

        [Theory]
        [InlineData(ClientMessageType.ProgramStarted, ClientMessageType.ProgramStarted)]
        [InlineData(ClientMessageType.ProgramFinished, ClientMessageType.ProgramFinished)]
        [InlineData(ClientMessageType.ProgramStopped, ClientMessageType.ProgramStopped)]
        [InlineData(ClientMessageType.ProgramDownloaded, ClientMessageType.ProgramTransferred)]
        [InlineData(ClientMessageType.RobotDebugMessage, ClientMessageType.RobotDebugMessage)]
        [InlineData(ClientMessageType.RobotError, ClientMessageType.RobotError)]
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
                ConvertMessageValuesToTestableValues(receivedMessage));
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
            Assert.Equal<string[]>(
                new[] { "ProgramId=>11235", "SourceClientId=>0", "SourceName=>Mihīni", "SourceType=>Robot" },
                ConvertMessageValuesToTestableValues(receivedMessage));
        }

        [Theory]
        [InlineData(ClientMessageType.StopProgram)]
        [InlineData(ClientMessageType.TransferProgram)]
        [InlineData(ClientMessageType.StartProgram)]
        [InlineData(ClientMessageType.StartMonitoring)]
        [InlineData(ClientMessageType.StopMonitoring)]
        [InlineData(ClientMessageType.AlertsRequest)]
        [InlineData(ClientMessageType.AlertBroadcast)]
        [InlineData(ClientMessageType.RobotStateUpdate)]
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
        [InlineData(ClientMessageType.StartProgram)]
        [InlineData(ClientMessageType.StartMonitoring)]
        [InlineData(ClientMessageType.StopMonitoring)]
        [InlineData(ClientMessageType.AlertsRequest)]
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
        [InlineData(ClientMessageType.AlertBroadcast)]
        [InlineData(ClientMessageType.RobotStateUpdate)]
        public async Task ProcessAsyncChecksConnectionIsForRobot(ClientMessageType messageType)
        {
            // Arrange
            var (_, processor, client) = InitialiseTestProcessor();
            client.User = new Data.User();

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
        [InlineData(ClientMessageType.StartProgram)]
        [InlineData(ClientMessageType.AlertsRequest)]
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
            Assert.Equal(new[] { "Robot is missing" }, RetrieveErrorMessages(msgs));
        }

        [Theory]
        [InlineData(ClientMessageType.StopProgram, ClientMessageType.ProgramStopped)]
        [InlineData(ClientMessageType.TransferProgram)]
        [InlineData(ClientMessageType.StartProgram)]
        [InlineData(ClientMessageType.AlertsRequest)]
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
            Assert.Equal(new[] { "Robot id is invalid" }, RetrieveErrorMessages(msgs));
        }

        [Theory]
        [InlineData(ClientMessageType.StopProgram, ClientMessageType.ProgramStopped)]
        [InlineData(ClientMessageType.TransferProgram)]
        [InlineData(ClientMessageType.StartProgram)]
        [InlineData(ClientMessageType.AlertsRequest)]
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
            Assert.Equal(new[] { "Robot is no longer connected" }, RetrieveErrorMessages(msgs));
        }

        [Theory]
        [InlineData(ClientMessageType.StopProgram, ClientMessageType.StopProgram, "")]
        [InlineData(ClientMessageType.TransferProgram, ClientMessageType.DownloadProgram, "program=14916")]
        [InlineData(ClientMessageType.StartProgram, ClientMessageType.StartProgram, "program=14916")]
        public async Task ProcessAsyncChecksRobotIsSentMessage(ClientMessageType inputMessage, ClientMessageType outputMessage, string parameters)
        {
            // Arrange
            var (hub, robotClient) = InitialiseHubConnection(14916);
            var (_, processor, client) = InitialiseTestProcessor(hub: hub);
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
        [InlineData(ClientMessageType.StartProgram, "program=14916", "INFORMATION: Starting program 14916 with {}")]
        public async Task ProcessAsyncWarnsIfRobotNotSet(ClientMessageType inputMessage, string parameters, params string[] extraMessages)
        {
            // Arrange
            var (hub, _) = InitialiseHubConnection(14916);
            var (_, processor, client) = InitialiseTestProcessor(hub: hub);
            client.User = new Data.User();

            // Act
            var msg = new ClientMessage(inputMessage, new { robot = "14916" });
            InitialiseMessageParameters(parameters, msg);
            await processor.ProcessAsync(client, msg);

            // Assert
            var messages = RetrieveLogMessages(processor);
            var expectedMessages = new List<string>();
            expectedMessages.Add($"INFORMATION: Processing message type {inputMessage}");
            expectedMessages.AddRange(extraMessages);
            expectedMessages.Add("WARNING: Unable to add to log: robot is missing");
            Assert.Equal(expectedMessages.ToArray(), messages);
        }

        [Theory]
        [InlineData(ClientMessageType.StopProgram, "Program stopping", "")]
        [InlineData(ClientMessageType.TransferProgram, "Program transferring", "program=14916")]
        [InlineData(ClientMessageType.StartProgram, "Program starting", "program=14916")]
        public async Task ProcessAsyncLogsMessage(ClientMessageType inputMessage, string logMessage, string parameters)
        {
            // Arrange
            var (hub, robotClient) = InitialiseHubConnection(14916);
            robotClient.Robot = new Data.Robot { MachineName = "Mihīni" };
            var (engine, processor, client) = InitialiseTestProcessor(hub: hub);
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

        [Theory]
        [InlineData(ClientMessageType.TransferProgram)]
        [InlineData(ClientMessageType.StartProgram)]
        public async Task ProcessAsyncRequiresProgramID(ClientMessageType inputMessage)
        {
            // Arrange
            var (hub, robotClient) = InitialiseHubConnection(14916);
            var (_, processor, client) = InitialiseTestProcessor(hub: hub);
            client.User = new Data.User();

            // Act
            var msg = new ClientMessage(inputMessage, new { robot = "14916" });
            await processor.ProcessAsync(client, msg);

            // Assert
            var msgs = client.RetrievePendingMessages();
            Assert.Equal(new[] { ClientMessageType.Error }, msgs.Select(m => m.Type).ToArray());
            Assert.Equal(new[] { "Program ID is missing" }, RetrieveErrorMessages(msgs));
        }

        [Theory]
        [InlineData(ClientMessageType.TransferProgram)]
        [InlineData(ClientMessageType.StartProgram)]
        public async Task ProcessAsyncChecksProgramID(ClientMessageType inputMessage)
        {
            // Arrange
            var (hub, robotClient) = InitialiseHubConnection(14916);
            var (_, processor, client) = InitialiseTestProcessor(hub: hub);
            client.User = new Data.User();

            // Act
            var msg = new ClientMessage(inputMessage, new { robot = "14916", program = "alpha" });
            await processor.ProcessAsync(client, msg);

            // Assert
            var msgs = client.RetrievePendingMessages();
            Assert.Equal(new[] { ClientMessageType.Error }, msgs.Select(m => m.Type).ToArray());
            Assert.Equal(new[] { "Program ID is invalid" }, RetrieveErrorMessages(msgs));
        }

        [Fact]
        public async Task RobotDebugMessageStoresSourceID()
        {
            // Arrange
            var (_, processor, client) = InitialiseTestProcessor();
            client.RobotDetails = new RobotStatus();

            // Act
            var msg = new ClientMessage(ClientMessageType.RobotDebugMessage, new { sourceID = "ABC1234" });
            await processor.ProcessAsync(client, msg);

            // Assert
            Assert.Contains("ABC1234", client!.RobotDetails!.SourceIds);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(" ")]
        public async Task RobotDebugMessageHandlesInvalidSourceID(string? id)
        {
            // Arrange
            var (_, processor, client) = InitialiseTestProcessor();
            client.RobotDetails = new RobotStatus();

            // Act
            var msg = new ClientMessage(ClientMessageType.RobotDebugMessage, new { sourceID = id });
            await processor.ProcessAsync(client, msg);

            // Assert
            Assert.Empty(client!.RobotDetails!.SourceIds);
        }

        [Fact]
        public async Task RobotDebugMessageHandlesMissingSourceID()
        {
            // Arrange
            var (_, processor, client) = InitialiseTestProcessor();
            client.RobotDetails = new RobotStatus();

            // Act
            var msg = new ClientMessage(ClientMessageType.RobotDebugMessage);
            await processor.ProcessAsync(client, msg);

            // Assert
            Assert.Empty(client!.RobotDetails!.SourceIds);
        }

        [Theory]
        [InlineData(ClientMessageType.StartMonitoring)]
        [InlineData(ClientMessageType.StopMonitoring)]
        public async Task MonitoringFailsIfNotConnectedToAHub(ClientMessageType inputType)
        {
            // Arrange
            var (_, processor, client) = InitialiseTestProcessor();
            client.User = new Data.User();

            // Act
            var msg = new ClientMessage(inputType);
            await processor.ProcessAsync(client, msg);

            // Assert
            var msgs = client.RetrievePendingMessages();
            Assert.Equal(new[] { ClientMessageType.Error }, msgs.Select(m => m.Type).ToArray());
            Assert.Equal(new[] { "Client not connected to Hub" }, RetrieveErrorMessages(msgs));
        }

        [Fact]
        public async Task StartMonitoringAddsClient()
        {
            // Arrange
            var hub = new Mock<IHub>();
            var (_, processor, client) = InitialiseTestProcessor();
            client.User = new Data.User();
            client.Hub = hub.Object;

            // Act
            var msg = new ClientMessage(ClientMessageType.StartMonitoring);
            await processor.ProcessAsync(client, msg);

            // Assert
            hub.Verify(h => h.AddClient(It.IsAny<ClientConnection>(), true));
        }

        [Fact]
        public async Task StopMonitoringRemovesClient()
        {
            // Arrange
            var hub = new Mock<IHub>();
            var (_, processor, client) = InitialiseTestProcessor();
            client.User = new Data.User();
            client.Hub = hub.Object;

            // Act
            var msg = new ClientMessage(ClientMessageType.StopMonitoring);
            await processor.ProcessAsync(client, msg);

            // Assert
            hub.Verify(h => h.RemoveClient(It.IsAny<ClientConnection>()));
        }

        [Fact]
        public async Task AlertsRequestSendsAllNotifications()
        {
            // Arrange
            var (hub, robotClient) = InitialiseHubConnection(14916);
            var (_, processor, client) = InitialiseTestProcessor(hub: hub);
            client.User = new Data.User();
            robotClient.AddNotification(new NotificationAlert { Id = 1, Message = "tahi", Severity = "mild" });
            robotClient.AddNotification(new NotificationAlert { Id = 2, Message = "rua", Severity = "hot" });

            // Act
            var msg = new ClientMessage(ClientMessageType.AlertsRequest, new { robot = "14916" });
            await processor.ProcessAsync(client, msg);

            // Assert
            var msgs = client.RetrievePendingMessages();
            Assert.Equal(
                new[] { ClientMessageType.AlertBroadcast, ClientMessageType.AlertBroadcast },
                msgs.Select(m => m.Type).ToArray());
            Assert.Equal(new[] { "1", "2" }, msgs.Select(m => m.Values["id"]).ToArray());
            Assert.Equal(new[] { "tahi", "rua" }, msgs.Select(m => m.Values["message"]).ToArray());
            Assert.Equal(new[] { "mild", "hot" }, msgs.Select(m => m.Values["severity"]).ToArray());
        }

        [Fact]
        public async Task AlertBroadcastAddsNotification()
        {
            // Arrange
            var (_, processor, client) = InitialiseTestProcessor();
            client.Robot = new Data.Robot();

            // Act
            var msg = new ClientMessage(ClientMessageType.AlertBroadcast);
            await processor.ProcessAsync(client, msg);

            // Assert
            Assert.NotEmpty(client.Notifications);
        }

        [Theory]
        [InlineData(null, -1)]
        [InlineData("", -1)]
        [InlineData("bad", -1)]
        [InlineData("2", 2)]
        public async Task AlertBroadcastSetsId(string? input, int expected)
        {
            // Arrange
            var (_, processor, client) = InitialiseTestProcessor();
            client.Robot = new Data.Robot();

            // Act
            var msg = new ClientMessage(ClientMessageType.AlertBroadcast, new { id = input });
            await processor.ProcessAsync(client, msg);

            // Assert
            var notification = client.Notifications.First();
            Assert.Equal(expected, notification.Id);
        }

        [Theory]
        [InlineData(null, "info")]
        [InlineData("", "info")]
        [InlineData("hot", "hot")]
        public async Task AlertBroadcastSetsSeverity(string? input, string expected)
        {
            // Arrange
            var (_, processor, client) = InitialiseTestProcessor();
            client.Robot = new Data.Robot();

            // Act
            var msg = new ClientMessage(ClientMessageType.AlertBroadcast, new { severity = input });
            await processor.ProcessAsync(client, msg);

            // Assert
            var notification = client.Notifications.First();
            Assert.Equal(expected, notification.Severity);
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("rua", "rua")]
        public async Task AlertBroadcastSetsMessage(string? input, string expected)
        {
            // Arrange
            var (_, processor, client) = InitialiseTestProcessor();
            client.Robot = new Data.Robot();

            // Act
            var msg = new ClientMessage(ClientMessageType.AlertBroadcast, new { message = input });
            await processor.ProcessAsync(client, msg);

            // Assert
            var notification = client.Notifications.First();
            Assert.Equal(expected, notification.Message);
        }

        [Fact]
        public async Task AlertBroadcastSendsToMonitors()
        {
            // Arrange
            var hub = new Mock<IHub>();
            ClientMessage? message = null;
            hub.Setup(h => h.SendToMonitors(It.IsAny<ClientMessage>()))
                .Callback((ClientMessage m) => message = m)
                .Verifiable();
            var (_, processor, client) = InitialiseTestProcessor(hub: hub.Object);
            client.Robot = new Data.Robot();

            // Act
            var msg = new ClientMessage(ClientMessageType.AlertBroadcast, new { id = "2", message = "rua", severity = "hot" });
            await processor.ProcessAsync(client, msg);

            // Assert
            hub.VerifyAll();
            Assert.Equal("2", message?.Values["id"]);
            Assert.Equal("rua", message?.Values["message"]);
            Assert.Equal("hot", message?.Values["severity"]);
        }

        [Fact]
        public async Task UpdateRobotStateSetsUnknownStatus()
        {
            // Arrange
            var (_, processor, client) = InitialiseTestProcessor();
            client.Robot = new Data.Robot();
            client.Status.Message = "Waiting";

            // Act
            var msg = new ClientMessage(ClientMessageType.RobotStateUpdate);
            await processor.ProcessAsync(client, msg);

            // Assert
            Assert.Equal("Unknown", client.Status.Message);
        }

        [Theory]
        [InlineData(null, "Unknown", false)]
        [InlineData("", "Unknown", false)]
        [InlineData(" ", "Unknown", false)]
        [InlineData("Running", "Running", false)]
        [InlineData("Waiting", "Waiting", true)]
        public async Task UpdateRobotStateSetsStatus(string? input, string expected, bool isAvailable)
        {
            // Arrange
            var (_, processor, client) = InitialiseTestProcessor();
            client.Robot = new Data.Robot();
            client.Status.Message = "Waiting";

            // Act
            var msg = new ClientMessage(ClientMessageType.RobotStateUpdate, new { state = input });
            await processor.ProcessAsync(client, msg);

            // Assert
            Assert.Equal(expected, client.Status.Message);
            Assert.Equal(isAvailable, client.Status.IsAvailable);
        }

        [Theory]
        [InlineData(null, "State updated to Unknown")]
        [InlineData("", "State updated to Unknown")]
        [InlineData(" ", "State updated to Unknown")]
        [InlineData("Executing", "State updated to Executing")]
        public async Task UpdateRobotStateLogsToRobotCallsCommand(string? input, string expected)
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
            var msg = new ClientMessage(ClientMessageType.RobotStateUpdate, new { state = input })
            {
                ConversationId = 963
            };
            await processor.ProcessAsync(client, msg);

            // Assert
            engine.Verify();
            Assert.Equal(expected, command?.Description);
        }

        [Fact]
        public async Task AuthenticateChecksForToken()
        {
            // Arrange
            var (_, processor, client) = InitialiseTestProcessor();

            // Act
            var msg = new ClientMessage(ClientMessageType.Authenticate);
            await processor.ProcessAsync(client, msg);

            // Assert
            var msgs = client.RetrievePendingMessages();
            Assert.Equal(new[] { ClientMessageType.Error }, msgs.Select(m => m.Type).ToArray());
            Assert.Equal(new[] { "Token is missing" }, RetrieveErrorMessages(msgs));
        }

        [Fact]
        public async Task AuthenticateValidatesToken()
        {
            // Arrange
            var (_, processor, client) = InitialiseTestProcessor();

            // Act
            var msg = new ClientMessage(ClientMessageType.Authenticate, new { token = "hacking" });
            await processor.ProcessAsync(client, msg);

            // Assert
            var msgs = client.RetrievePendingMessages();
            Assert.Equal(new[] { ClientMessageType.Error }, msgs.Select(m => m.Type).ToArray());
            Assert.Equal(new[] { "Token is invalid" }, RetrieveErrorMessages(msgs));
        }

        [Fact]
        public async Task AuthenticateChecksForSession()
        {
            // Arrange
            var (_, processor, client) = InitialiseTestProcessor();
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                })
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Act
            var msg = new ClientMessage(ClientMessageType.Authenticate, new { token = tokenHandler.WriteToken(token) });
            await processor.ProcessAsync(client, msg);

            // Assert
            var msgs = client.RetrievePendingMessages();
            Assert.Equal(new[] { ClientMessageType.Error }, msgs.Select(m => m.Type).ToArray());
            Assert.Equal(new[] { "Token is invalid: missing session" }, RetrieveErrorMessages(msgs));
        }

        [Fact]
        public async Task AuthenticateChecksSessionExists()
        {
            // Arrange
            var (engine, processor, client) = InitialiseTestProcessor();
            var token = GenerateJwtToken(sessionId);
            var sessionQuery = new Mock<SessionData>();
            engine.RegisterQuery(sessionQuery.Object);

            // Act
            var msg = new ClientMessage(ClientMessageType.Authenticate, new { token });
            await processor.ProcessAsync(client, msg);

            // Assert
            var msgs = client.RetrievePendingMessages();
            Assert.Equal(new[] { ClientMessageType.Error }, msgs.Select(m => m.Type).ToArray());
            Assert.Equal(new[] { "Session is invalid" }, RetrieveErrorMessages(msgs));
        }

        [Fact]
        public async Task AuthenticateChecksSessionExpiresInFuture()
        {
            // Arrange
            var (engine, processor, client) = InitialiseTestProcessor();
            var token = GenerateJwtToken(sessionId);
            var sessionQuery = new Mock<SessionData>();
            sessionQuery.Setup(q => q.RetrieveByIdAsync(sessionId))
                .Returns(Task.FromResult<Data.Session?>(new Data.Session { WhenExpires = DateTime.MinValue }));
            engine.RegisterQuery(sessionQuery.Object);

            // Act
            var msg = new ClientMessage(ClientMessageType.Authenticate, new { token });
            await processor.ProcessAsync(client, msg);

            // Assert
            var msgs = client.RetrievePendingMessages();
            Assert.Equal(new[] { ClientMessageType.Error }, msgs.Select(m => m.Type).ToArray());
            Assert.Equal(new[] { "Session is invalid" }, RetrieveErrorMessages(msgs));
        }

        [Fact]
        public async Task AuthenticateChecksSessionHasUserId()
        {
            // Arrange
            var (engine, processor, client) = InitialiseTestProcessor();
            var token = GenerateJwtToken(sessionId);
            var sessionQuery = new Mock<SessionData>();
            sessionQuery.Setup(q => q.RetrieveByIdAsync(sessionId))
                .Returns(Task.FromResult<Data.Session?>(new Data.Session { WhenExpires = DateTime.MaxValue }));
            engine.RegisterQuery(sessionQuery.Object);

            // Act
            var msg = new ClientMessage(ClientMessageType.Authenticate, new { token });
            await processor.ProcessAsync(client, msg);

            // Assert
            var msgs = client.RetrievePendingMessages();
            Assert.Equal(new[] { ClientMessageType.Error }, msgs.Select(m => m.Type).ToArray());
            Assert.Equal(new[] { "Session is invalid: missing id" }, RetrieveErrorMessages(msgs));
        }

        [Fact]
        public async Task AuthenticateChecksRobotExists()
        {
            // Arrange
            var (engine, processor, client) = InitialiseTestProcessor();
            var token = GenerateJwtToken(sessionId);
            var sessionQuery = new Mock<SessionData>();
            sessionQuery.Setup(q => q.RetrieveByIdAsync(sessionId))
                .Returns(Task.FromResult<Data.Session?>(new Data.Session
                {
                    WhenExpires = DateTime.MaxValue,
                    IsRobot = true,
                    UserId = "robots/1"
                }));
            engine.RegisterQuery(sessionQuery.Object);
            var robotQuery = new Mock<RobotData>();
            engine.RegisterQuery(robotQuery.Object);

            // Act
            var msg = new ClientMessage(ClientMessageType.Authenticate, new { token });
            await processor.ProcessAsync(client, msg);

            // Assert
            var msgs = client.RetrievePendingMessages();
            Assert.Equal(new[] { ClientMessageType.Error }, msgs.Select(m => m.Type).ToArray());
            Assert.Equal(new[] { "Session is invalid: missing robot" }, RetrieveErrorMessages(msgs));
        }

        [Fact]
        public async Task AuthenticateReturnsSessionForRobot()
        {
            // Arrange
            var (engine, processor, client) = InitialiseTestProcessor();
            var token = GenerateJwtToken(sessionId);
            var sessionQuery = new Mock<SessionData>();
            sessionQuery.Setup(q => q.RetrieveByIdAsync(sessionId))
                .Returns(Task.FromResult<Data.Session?>(new Data.Session
                {
                    WhenExpires = DateTime.MaxValue,
                    IsRobot = true,
                    UserId = "robots/1"
                }));
            engine.RegisterQuery(sessionQuery.Object);
            var robotQuery = new Mock<RobotData>();
            robotQuery.Setup(q => q.RetrieveByIdAsync("robots/1"))
                .Returns(Task.FromResult<Data.Robot?>(new Data.Robot()));
            engine.RegisterQuery(robotQuery.Object);

            // Act
            var msg = new ClientMessage(ClientMessageType.Authenticate, new { token });
            await processor.ProcessAsync(client, msg);

            // Assert
            var msgs = client.RetrievePendingMessages();
            Assert.Equal(new[] { ClientMessageType.Authenticated }, msgs.Select(m => m.Type).ToArray());
        }

        [Fact]
        public async Task AuthenticateSetsRobotValues()
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
            var (engine, processor, client) = InitialiseTestProcessor(hub: hub.Object);
            var token = GenerateJwtToken(sessionId);
            var sessionQuery = new Mock<SessionData>();
            sessionQuery.Setup(q => q.RetrieveByIdAsync(sessionId))
                .Returns(Task.FromResult<Data.Session?>(new Data.Session
                {
                    WhenExpires = DateTime.MaxValue,
                    IsRobot = true,
                    UserId = "robots/1"
                }));
            engine.RegisterQuery(sessionQuery.Object);
            var robotQuery = new Mock<RobotData>();
            robotQuery.Setup(q => q.RetrieveByIdAsync("robots/1"))
                .Returns(Task.FromResult<Data.Robot?>(new Data.Robot { FriendlyName = "Mia" }));
            engine.RegisterQuery(robotQuery.Object);

            // Act
            var msg = new ClientMessage(ClientMessageType.Authenticate, new { token });
            await processor.ProcessAsync(client, msg);

            // Assert
            Assert.Equal(
                new[] { "ClientId=>0", "Name=>Mia", "SubType=>Unknown", "Type=>robot" },
                ConvertMessageValuesToTestableValues(receivedMessage));
        }

        [Fact]
        public async Task AuthenticateSetsRobotValuesWithRobotType()
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
            var (engine, processor, client) = InitialiseTestProcessor(hub: hub.Object);
            var token = GenerateJwtToken(sessionId);
            var sessionQuery = new Mock<SessionData>();
            sessionQuery.Setup(q => q.RetrieveByIdAsync(sessionId))
                .Returns(Task.FromResult<Data.Session?>(new Data.Session
                {
                    WhenExpires = DateTime.MaxValue,
                    IsRobot = true,
                    UserId = "robots/1"
                }));
            engine.RegisterQuery(sessionQuery.Object);
            var robotQuery = new Mock<RobotData>();
            robotQuery.Setup(q => q.RetrieveByIdAsync("robots/1"))
                .Returns(Task.FromResult<Data.Robot?>(new Data.Robot { 
                    FriendlyName = "Mia",
                    Type = new Data.RobotType { Name = "Mihīni" }
                }));
            engine.RegisterQuery(robotQuery.Object);

            // Act
            var msg = new ClientMessage(ClientMessageType.Authenticate, new { token });
            await processor.ProcessAsync(client, msg);

            // Assert
            Assert.Equal(
                new[] { "ClientId=>0", "Name=>Mia", "SubType=>Mihīni", "Type=>robot" },
                ConvertMessageValuesToTestableValues(receivedMessage));
        }

        [Fact]
        public async Task AuthenticateChecksUserExists()
        {
            // Arrange
            var (engine, processor, client) = InitialiseTestProcessor();
            var token = GenerateJwtToken(sessionId);
            var sessionQuery = new Mock<SessionData>();
            sessionQuery.Setup(q => q.RetrieveByIdAsync(sessionId))
                .Returns(Task.FromResult<Data.Session?>(new Data.Session
                {
                    WhenExpires = DateTime.MaxValue,
                    IsRobot = false,
                    UserId = "users/1"
                }));
            engine.RegisterQuery(sessionQuery.Object);
            var userQuery = new Mock<UserData>();
            engine.RegisterQuery(userQuery.Object);

            // Act
            var msg = new ClientMessage(ClientMessageType.Authenticate, new { token });
            await processor.ProcessAsync(client, msg);

            // Assert
            var msgs = client.RetrievePendingMessages();
            Assert.Equal(new[] { ClientMessageType.Error }, msgs.Select(m => m.Type).ToArray());
            Assert.Equal(new[] { "Session is invalid: missing user" }, RetrieveErrorMessages(msgs));
        }

        [Fact]
        public async Task AuthenticateChecksStartsConversation()
        {
            // Arrange
            var (engine, processor, client) = InitialiseTestProcessor();
            var token = GenerateJwtToken(sessionId);
            var sessionQuery = new Mock<SessionData>();
            sessionQuery.Setup(q => q.RetrieveByIdAsync(sessionId))
                .Returns(Task.FromResult<Data.Session?>(new Data.Session
                {
                    WhenExpires = DateTime.MaxValue,
                    IsRobot = false,
                    UserId = "users/1"
                }));
            engine.RegisterQuery(sessionQuery.Object);
            var userQuery = new Mock<UserData>();
            userQuery.Setup(q => q.RetrieveByIdAsync("users/1"))
                .Returns(Task.FromResult<Data.User?>(new Data.User { Name = "Mia" }));
            engine.RegisterQuery(userQuery.Object);
            engine.ExpectCommand<StartConversation>();
            engine.OnExecute = c => CommandResult.New(1, new Data.Conversation { ConversationId = 1 });

            // Act
            var msg = new ClientMessage(ClientMessageType.Authenticate, new { token });
            await processor.ProcessAsync(client, msg);

            // Assert
            var msgs = client.RetrievePendingMessages();
            Assert.Equal(new[] { ClientMessageType.Authenticated }, msgs.Select(m => m.Type).ToArray());
            engine.Verify();
        }

        [Fact]
        public async Task AuthenticateHandlesStartsConversationError()
        {
            // Arrange
            var (engine, processor, client) = InitialiseTestProcessor();
            var token = GenerateJwtToken(sessionId);
            var sessionQuery = new Mock<SessionData>();
            sessionQuery.Setup(q => q.RetrieveByIdAsync(sessionId))
                .Returns(Task.FromResult<Data.Session?>(new Data.Session
                {
                    WhenExpires = DateTime.MaxValue,
                    IsRobot = false,
                    UserId = "users/1"
                }));
            engine.RegisterQuery(sessionQuery.Object);
            var userQuery = new Mock<UserData>();
            userQuery.Setup(q => q.RetrieveByIdAsync("users/1"))
                .Returns(Task.FromResult<Data.User?>(new Data.User { Name = "Mia" }));
            engine.RegisterQuery(userQuery.Object);
            engine.ExpectCommand<StartConversation>();
            engine.OnExecute = c => new CommandResult(1, "Failed");

            // Act
            var msg = new ClientMessage(ClientMessageType.Authenticate, new { token });
            await processor.ProcessAsync(client, msg);

            // Assert
            var msgs = client.RetrievePendingMessages();
            Assert.Equal(new[] { ClientMessageType.Error }, msgs.Select(m => m.Type).ToArray());
            Assert.Equal(new[] { "Session is invalid: cannot start conversation" }, RetrieveErrorMessages(msgs));
        }

        [Fact]
        public async Task AuthenticateSetsUserValues()
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
            var (engine, processor, client) = InitialiseTestProcessor(hub: hub.Object);
            var token = GenerateJwtToken(sessionId);
            var sessionQuery = new Mock<SessionData>();
            sessionQuery.Setup(q => q.RetrieveByIdAsync(sessionId))
                .Returns(Task.FromResult<Data.Session?>(new Data.Session
                {
                    WhenExpires = DateTime.MaxValue,
                    IsRobot = false,
                    UserId = "users/1"
                }));
            engine.RegisterQuery(sessionQuery.Object);
            var userQuery = new Mock<UserData>();
            userQuery.Setup(q => q.RetrieveByIdAsync("users/1"))
                .Returns(Task.FromResult<Data.User?>(new Data.User { Name = "Mia" }));
            engine.RegisterQuery(userQuery.Object);
            engine.ExpectCommand<StartConversation>();
            engine.OnExecute = c => CommandResult.New(1, new Data.Conversation { ConversationId = 1 });

            // Act
            var msg = new ClientMessage(ClientMessageType.Authenticate, new { token });
            await processor.ProcessAsync(client, msg);

            // Assert
            Assert.Equal(
                new[] { "ClientId=>0", "IsStudent=>yes", "Name=>Mia", "SubType=>Student", "Type=>user" },
                ConvertMessageValuesToTestableValues(receivedMessage));
        }

        private static string GenerateJwtToken(string sessionId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("SessionId", sessionId)
                })
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static string[] RetrieveErrorMessages(IEnumerable<ClientMessage> msgs)
        {
            return msgs.Where(m => m.Values.ContainsKey("error")).Select(m => m.Values["error"]).ToArray();
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

        private static (IHub, ClientConnection) InitialiseHubConnection(long id)
        {
            var client = InitialiseClient();
            var hub = new Mock<IHub>();
            hub.Setup(h => h.GetClient(id)).Returns(client);
            return (hub.Object, client);
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

        private static string[] ConvertMessageValuesToTestableValues(ClientMessage? message)
        {
            if (message == null) return Array.Empty<string>();

            return message.Values.Select(value => $"{value.Key}=>{value.Value}").OrderBy(v => v).ToArray();
        }
    }
}
