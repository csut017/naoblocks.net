using Microsoft.AspNetCore.Mvc;
using Moq;
using NaoBlocks.Common;
using NaoBlocks.Web.Communications;
using NaoBlocks.Web.Controllers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class ClientsControllerTests
    {
        [Fact]
        public async Task GetLogRetrievesLogs()
        {
            // Arrange
            var logger = new FakeLogger<ClientsController>();
            var hub = new Mock<IHub>();
            IReadOnlyCollection<ClientMessage> messages = new ReadOnlyCollection<ClientMessage>(new[] {
                    new ClientMessage(),
                    new ClientMessage()
                });
            var client = new Mock<IClientConnection>();
            client.Setup(c => c.GetMessageLogAsync())
                .Returns(Task.FromResult(messages));
            hub.Setup(h => h.GetClient(1))
                .Returns(client.Object);
            var controller = new ClientsController(
                logger,
                hub.Object);

            // Act
            var response = await controller.GetLog("1");

            // Assert
            Assert.NotNull(response.Value);
            Assert.Equal(2, response.Value?.Count);
            Assert.Equal(0, response.Value?.Page);
            Assert.NotEmpty(response.Value?.Items);
        }

        [Fact]
        public async Task GetLogHandlesMissingConnection()
        {
            // Arrange
            var logger = new FakeLogger<ClientsController>();
            var hub = new Mock<IHub>();
            hub.Setup(h => h.GetClient(1))
                .Returns((IClientConnection?)null);
            var controller = new ClientsController(
                logger,
                hub.Object);

            // Act
            var response = await controller.GetLog("1");

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task GetLogValiatesData()
        {
            // Arrange
            var logger = new FakeLogger<ClientsController>();
            var hub = new Mock<IHub>();
            var controller = new ClientsController(
                logger,
                hub.Object);

            // Act
            var response = await controller.GetLog("bad");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public void ListRetrievesViaQuery()
        {
            // Arrange
            var logger = new FakeLogger<ClientsController>();
            var hub = new Mock<IHub>();
            hub.Setup(h => h.GetClients(ClientConnectionType.Robot))
                .Returns(new[] {
                    new StandardClientConnection(
                        new Mock<WebSocket>().Object,
                        ClientConnectionType.Robot,
                        new Mock<IMessageProcessor>().Object)
                });
            var controller = new ClientsController(
                logger,
                hub.Object);

            // Act
            var response = controller.List(ClientConnectionType.Robot, null, null);

            // Assert
            Assert.Equal(1, response.Value?.Count);
            Assert.Equal(0, response.Value?.Page);
            Assert.NotEmpty(response.Value?.Items);
        }

        [Fact]
        public void ListHandlesNullData()
        {
            // Arrange
            var logger = new FakeLogger<ClientsController>();
            var hub = new Mock<IHub>();
            hub.Setup(h => h.GetClients(ClientConnectionType.Robot));
            var controller = new ClientsController(
                logger,
                hub.Object);

            // Act
            var response = controller.List(ClientConnectionType.Robot, null, null);

            // Assert
            Assert.Equal(0, response.Value?.Count);
            Assert.Equal(0, response.Value?.Page);
            Assert.Empty(response.Value?.Items);
        }
    }
}
