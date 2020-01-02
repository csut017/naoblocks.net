using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NaoBlocks.Web.Communications;
using NaoBlocks.Web.Communications.Messages;
using System.Threading;
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

        [HttpGet("user")]
        [AllowAnonymous]
        public async Task StartUser()
        {
            var context = ControllerContext.HttpContext;
            var isSocketRequest = context.WebSockets.IsWebSocketRequest;

            if (isSocketRequest)
            {
                this._logger.LogInformation($"Accepting web socket request");
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                using (var client = new ClientConnection(webSocket, ClientConnectionType.User, this._messageProcessor))
                {
                    this._hub.AddClient(client);
                    await client.StartAsync(CancellationToken.None);
                }
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }
    }
}