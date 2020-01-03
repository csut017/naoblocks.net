using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NaoBlocks.Web.Communications;
using System;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class ConnectionsController

        : ControllerBase
    {
        private readonly IHub _hub;
        private readonly ILogger<ConnectionsController> _logger;
        private readonly IMessageProcessor _messageProcessor;

        public ConnectionsController(ILogger<ConnectionsController> logger, IHub hub, IMessageProcessor messageProcessor)
        {
            this._logger = logger;
            this._hub = hub;
            this._messageProcessor = messageProcessor;
        }

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
                using (var client = new ClientConnection(webSocket, clientType, this._messageProcessor))
                {
                    this._hub.AddClient(client);
                    await client.StartAsync();
                }
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }
    }
}