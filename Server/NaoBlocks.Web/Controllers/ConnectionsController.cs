using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Web.Communications;
using System.Net.WebSockets;

namespace NaoBlocks.Web.Controllers
{
    /// <summary>
    /// Controller for initialising web socket connections.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class ConnectionsController

        : ControllerBase
    {
        private readonly IHub _hub;
        private readonly ILogger<ConnectionsController> _logger;
        private readonly IMessageProcessor _messageProcessor;

        /// <summary>
        /// Initialises a new <see cref="ConnectionsController"/> instance.
        /// </summary>
        /// <param name="logger">The logger instance to use.</param>
        /// <param name="hub">The communications hub.</param>
        /// <param name="messageProcessor">The message processor for processing incoming messages.</param>
        /// <param name="services">An <see cref="IServiceCollection"/> for initialising new services.</param>
        public ConnectionsController(ILogger<ConnectionsController> logger, IHub hub, IMessageProcessor messageProcessor, IServiceProvider services)
        {
            this._logger = logger;
            this._hub = hub;
            this._messageProcessor = messageProcessor;
            this.GenerateConnection =
                (socket, type, processor) => new WebSocketClientConnection(
                    socket,
                    type,
                    processor,
                    services.GetService<ILogger<WebSocketClientConnection>>()!);
        }

        /// <summary>
        /// Gets or sets the connection generator.
        /// </summary>
        /// <remarks>
        /// This property is mainly to allow unit testing.
        /// </remarks>
        public Func<WebSocket, ClientConnectionType, IMessageProcessor, IClientConnection> GenerateConnection { get; set; }

        /// <summary>
        /// Attempts to start a new connection.
        /// </summary>
        /// <param name="type">The type of connection.</param>
        [HttpGet("{type}")]
        [AllowAnonymous]
        public async Task Start(string type)
        {
            var context = ControllerContext.HttpContext;
            if (!Enum.TryParse<ClientConnectionType>(type, true, out ClientConnectionType clientType))
            {
                context.Response.StatusCode = 404;
                return;
            }

            var isSocketRequest = context.WebSockets.IsWebSocketRequest;
            if (isSocketRequest)
            {
                this._logger.LogInformation($"Accepting web socket request from {context.Connection.RemoteIpAddress}");
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var client = this.GenerateConnection(webSocket, clientType, this._messageProcessor);
                this._hub.AddClient(client);
                if (client is IStartableClientConnection startable) await startable.StartAsync();
                client.Dispose();
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }
    }
}