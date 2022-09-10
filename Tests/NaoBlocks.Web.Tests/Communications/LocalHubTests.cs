using Moq;
using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using NaoBlocks.Web.Communications;
using System;
using System.Linq;
using System.Net.WebSockets;
using Xunit;

namespace NaoBlocks.Web.Tests.Communications
{
    public class LocalHubTests
    {
        [Fact]
        public void AddClientAddsClient()
        {
            // Arrange
            using var hub = new LocalHub(new FakeLogger<LocalHub>());
            var client = InitialiseClient();

            // Act
            hub.AddClient(client);
            var actual = hub.GetClient(client.Id);

            // Assert
            Assert.Same(actual, client);
        }

        [Fact]
        public void AddClientSetsClientId()
        {
            // Arrange
            using var hub = new LocalHub(new FakeLogger<LocalHub>());
            var client = InitialiseClient();

            // Act
            hub.AddClient(InitialiseClient());
            hub.AddClient(client);

            // Assert
            Assert.Equal(2, client.Id);
        }

        [Fact]
        public void ClosingAClientRemovesIt()
        {
            // Arrange
            using var hub = new LocalHub(new FakeLogger<LocalHub>());
            var client = InitialiseClient();
            hub.AddClient(client);

            // Act
            client.Close();

            // Assert
            Assert.Empty(hub.GetAllClients());
        }

        [Fact]
        public void GetClientHandlesMissingClient()
        {
            // Arrange
            using var hub = new LocalHub(new FakeLogger<LocalHub>());

            // Act
            var actual = hub.GetClient(1);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void GetClientsReturnsOnlyClientsOfType()
        {
            // Arrange
            using var hub = new LocalHub(new FakeLogger<LocalHub>());
            var client = InitialiseClient(ClientConnectionType.Robot);
            hub.AddClient(client);

            // Assert
            Assert.DoesNotContain(client, hub.GetClients(ClientConnectionType.User));
            Assert.Contains(client, hub.GetClients(ClientConnectionType.Robot));
        }

        [Fact]
        public void GetAllClientsReturnsAllClients()
        {
            // Arrange
            using var hub = new LocalHub(new FakeLogger<LocalHub>());
            var robot = InitialiseClient(ClientConnectionType.Robot);
            var user = InitialiseClient(ClientConnectionType.User);
            hub.AddClient(robot);
            hub.AddClient(user);

            // Act
            var clients = hub.GetAllClients();

            // Assert
            Assert.Contains(robot, clients);
            Assert.Contains(user, clients);
        }

        [Fact]
        public void RemoveClientRemovesClient()
        {
            // Arrange
            using var hub = new LocalHub(new FakeLogger<LocalHub>());
            var client = InitialiseClient(ClientConnectionType.Robot);
            hub.AddClient(client);

            // Act
            hub.RemoveClient(client);
            var actual = hub.GetClient(1);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void AddClientMessagesMonitors()
        {
            // Arrange
            using var hub = new LocalHub(new FakeLogger<LocalHub>());
            var monitor = InitialiseClient(ClientConnectionType.User);
            var client = InitialiseClient(ClientConnectionType.Robot);
            hub.AddMonitor(monitor);

            // Act
            hub.AddClient(client);

            // Assert
            Assert.Equal(
                new[] { ClientMessageType.ClientAdded },
                monitor.RetrievePendingMessages().Select(m => m.Type).ToArray());
            Assert.Equal(
                new[] { "1" },
                monitor.RetrievePendingMessages().Where(m => m.Values.ContainsKey("ClientId")).Select(m => m.Values["ClientId"]).ToArray());
        }

        [Fact]
        public void RemoveClientMessagesMonitors()
        {
            // Arrange
            using var hub = new LocalHub(new FakeLogger<LocalHub>());
            var client = InitialiseClient(ClientConnectionType.Robot);
            var monitor = InitialiseClient(ClientConnectionType.User);
            hub.AddClient(client);
            hub.AddMonitor(monitor);

            // Act
            hub.RemoveClient(client);
            var actual = hub.GetClient(1);

            // Assert
            Assert.Equal(
                new[] { ClientMessageType.ClientAdded, ClientMessageType.ClientRemoved },
                monitor.RetrievePendingMessages().Select(m => m.Type).ToArray());
            Assert.Equal(
                new[] { "1", "1" },
                monitor.RetrievePendingMessages().Where(m => m.Values.ContainsKey("ClientId")).Select(m => m.Values["ClientId"]).ToArray());
        }

        [Fact]
        public void SendToMonitorsHandlesNoMonitors()
        {
            // Arrange
            using var hub = new LocalHub(new FakeLogger<LocalHub>());
            var client = InitialiseClient(ClientConnectionType.Robot);
            hub.AddClient(client);

            // Act
            hub.SendToMonitors(new ClientMessage(ClientMessageType.Unknown));
        }

        [Fact]
        public void SendToMonitorsSendsMessage()
        {
            // Arrange
            using var hub = new LocalHub(new FakeLogger<LocalHub>());
            var client = InitialiseClient(ClientConnectionType.Robot);
            var monitor = InitialiseClient(ClientConnectionType.User);
            hub.AddClient(client);
            hub.AddMonitor(monitor);

            // Act
            var msg = new ClientMessage(ClientMessageType.ProgramStarted);
            hub.SendToMonitors(msg);

            // Assert
            Assert.Equal(
                new[] { ClientMessageType.ClientAdded, ClientMessageType.ProgramStarted },
                monitor.RetrievePendingMessages().Select(m => m.Type).ToArray());
        }

        [Fact]
        public void SendToAllSendsMessage()
        {
            // Arrange
            using var hub = new LocalHub(new FakeLogger<LocalHub>());
            var robot = InitialiseClient(ClientConnectionType.Robot);
            var user = InitialiseClient(ClientConnectionType.User);
            hub.AddClient(robot);
            hub.AddClient(user);

            // Act
            var msg = new ClientMessage(ClientMessageType.ProgramStarted);
            hub.SendToAll(msg);

            // Assert
            Assert.Equal(
                new[] { ClientMessageType.ProgramStarted },
                user.RetrievePendingMessages().Select(m => m.Type).ToArray());
            Assert.Equal(
                new[] { ClientMessageType.ProgramStarted },
                robot.RetrievePendingMessages().Select(m => m.Type).ToArray());
        }

        [Fact]
        public void SendToAllWithTypeSendsMessageToOnlyType()
        {
            // Arrange
            using var hub = new LocalHub(new FakeLogger<LocalHub>());
            var robot = InitialiseClient(ClientConnectionType.Robot);
            var user = InitialiseClient(ClientConnectionType.User);
            hub.AddClient(robot);
            hub.AddClient(user);

            // Act
            var msg = new ClientMessage(ClientMessageType.ProgramStarted);
            hub.SendToAll(msg, ClientConnectionType.Robot);

            // Assert
            Assert.Empty(user.RetrievePendingMessages().ToArray());
            Assert.Equal(
                new[] { ClientMessageType.ProgramStarted },
                robot.RetrievePendingMessages().Select(m => m.Type).ToArray());
        }

        [Fact]
        public void AddMonitorAddsMonitor()
        {
            // Arrange
            using var hub = new LocalHub(new FakeLogger<LocalHub>());
            var client = InitialiseClient();

            // Act
            hub.AddMonitor(client);
            var actual = hub.GetMonitors().FirstOrDefault();

            // Assert
            Assert.Same(actual, client);
        }

        [Fact]
        public void AddMonitorNotifiesMonitorAboutUsers()
        {
            // Arrange
            using var hub = new LocalHub(new FakeLogger<LocalHub>());
            var client = InitialiseClient(ClientConnectionType.User);
            client.User = new User { Name = "Mia", Role = UserRole.Student };
            hub.AddClient(client);
            var monitor = InitialiseClient();

            // Act
            hub.AddMonitor(monitor);

            // Assert
            var msg = monitor.RetrievePendingMessages().First();
            Assert.Equal(ClientMessageType.ClientAdded, msg.Type);
            Assert.Equal(
                new[] { "ClientId=>1", "Name=>Mia", "SubType=>Student", "Type=>user" },
                ConvertMessageValuesToTestableValues(msg));
        }

        [Fact]
        public void AddMonitorNotifiesMonitorAboutRobots()
        {
            // Arrange
            using var hub = new LocalHub(new FakeLogger<LocalHub>());
            var client = InitialiseClient(ClientConnectionType.Robot);
            client.Robot = new Robot
            {
                FriendlyName = "Mihīni",
                Type = new RobotType { Name = "Nao" }
            };
            hub.AddClient(client);
            var monitor = InitialiseClient();

            // Act
            hub.AddMonitor(monitor);

            // Assert
            var msg = monitor.RetrievePendingMessages().First();
            Assert.Equal(ClientMessageType.ClientAdded, msg.Type);
            Assert.Equal(
                new[] { "ClientId=>1", "Name=>Mihīni", "state=>Unknown", "SubType=>Nao", "Type=>robot" },
                ConvertMessageValuesToTestableValues(msg));
        }

        [Fact]
        public void RemoveMonitorRemovesMonitor()
        {
            // Arrange
            using var hub = new LocalHub(new FakeLogger<LocalHub>());
            var client = InitialiseClient();

            // Act
            hub.AddMonitor(client);
            hub.RemoveMonitor(client);

            // Assert
            Assert.Empty(hub.GetMonitors());
        }

        [Fact]
        public void ClosingMonitorRemovesMonitor()
        {
            // Arrange
            using var hub = new LocalHub(new FakeLogger<LocalHub>());
            var client = InitialiseClient();

            // Act
            hub.AddMonitor(client);
            client.Close();

            // Assert
            Assert.Empty(hub.GetMonitors());
        }

        private static WebSocketClientConnection InitialiseClient(ClientConnectionType type = ClientConnectionType.Unknown)
        {
            var hub = new Mock<IHub>();
            var logger = new FakeLogger<MessageProcessor>();
            var factory = new Mock<IEngineFactory>();
            var socket = new Mock<WebSocket>();
            var processor = new MessageProcessor(hub.Object, logger, factory.Object);
            var client = new WebSocketClientConnection(socket.Object, type, processor, new FakeLogger<WebSocketClientConnection>());
            return client;
        }
        private static string[] ConvertMessageValuesToTestableValues(ClientMessage? message)
        {
            if (message == null) return Array.Empty<string>();

            return message.Values.Select(value => $"{value.Key}=>{value.Value}").OrderBy(v => v).ToArray();
        }
    }
}
