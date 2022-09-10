using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NaoBlocks.Web.Communications;
using NaoBlocks.Web.Controllers;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class ConnectionsControllerTests
    {
        [Fact]
        public async Task StartCheckForWebSocketRequest()
        {
            // Arrange
            var logger = new FakeLogger<ConnectionsController>();
            var hub = new Mock<IHub>();
            var processor = new Mock<IMessageProcessor>();
            var controller = new ConnectionsController(logger, hub.Object, processor.Object, GenerateServiceProvider());
            var response = SetupControllerContext(controller, false);

            // Act
            await controller.Start("user");

            // Assert
            Assert.Equal(400, response.StatusCode);
        }

        [Fact]
        public async Task StartChecksClientType()
        {
            // Arrange
            var logger = new FakeLogger<ConnectionsController>();
            var hub = new Mock<IHub>();
            var processor = new Mock<IMessageProcessor>();
            var controller = new ConnectionsController(logger, hub.Object, processor.Object, GenerateServiceProvider());
            var response = SetupControllerContext(controller, false);

            // Act
            await controller.Start("rubbish");

            // Assert
            Assert.Equal(404, response.StatusCode);
        }

        [Fact]
        public async Task StartInitialisesConnection()
        {
            // Arrange
            var logger = new FakeLogger<ConnectionsController>();
            var hub = new Mock<IHub>();
            var processor = new Mock<IMessageProcessor>();
            var controller = new ConnectionsController(logger, hub.Object, processor.Object, GenerateServiceProvider());
            SetupControllerContext(controller, true);

            var connection = new Mock<IClientConnection>();
            connection
                .Setup(c => c.StartAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();
            connection
                .Setup(c => c.Dispose())
                .Verifiable();
            controller.GenerateConnection = (s, t, p) => connection.Object;

            // Act
            await controller.Start("user");

            // Assert
            connection.Verify();
        }

        private static IServiceProvider GenerateServiceProvider()
        {
            var fake = new Mock<IServiceProvider>();
            fake.Setup(s => s.GetService(typeof(ILogger<WebSocketClientConnection>)))
                .Returns(new FakeLogger<SocketClientConnection>());
            return fake.Object;
        }

        private static HttpResponse SetupControllerContext(ConnectionsController controller, bool isWebsocket)
        {
            var webSocket = new Mock<WebSocket>();
            var webSocketManager = new Mock<WebSocketManager>();
            webSocketManager
                .Setup(x => x.IsWebSocketRequest)
                .Returns(isWebsocket);
            webSocketManager
                .Setup(x => x.AcceptWebSocketAsync())
                .Returns(Task.FromResult(webSocket.Object));

            var response = new Mock<HttpResponse>();
            response
                .SetupProperty(r => r.StatusCode);

            var info = new Mock<ConnectionInfo>();
            info
                .Setup(i => i.RemoteIpAddress)
                .Returns(new IPAddress(0));

            var httpContext = new Mock<HttpContext>();
            httpContext
                .Setup(c => c.Response)
                .Returns(response.Object);
            httpContext
                .Setup(c => c.WebSockets)
                .Returns(webSocketManager.Object);
            httpContext
                .Setup(c => c.Connection)
                .Returns(info.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext.Object
            };

            return response.Object;
        }
    }
}