using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NaoBlocks.Common;
using NaoBlocks.Core.Models;
using NaoBlocks.Web.Communications;
using NaoBlocks.Web.Dtos;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientsController: ControllerBase
    {
        private readonly IHub _hub;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(ILogger<ClientsController> logger, IHub hub)
        {
            this._logger = logger;
            this._hub = hub;
        }

        [HttpGet("{type}")]
        public ActionResult<ListResult<HubClient>> List(ClientConnectionType type)
        {
            this._logger.LogDebug($"Listing connections");
            return ListResult.New(this._hub.GetClients(type).Where(c => c != null).Select(HubClient.FromModel));
        }

        [HttpGet("{id}/logs")]
        public async Task<ActionResult<ListResult<ClientMessage>>> GetLogs(string id)
        {
            if (!long.TryParse(id, out long clientId))
            {
                return BadRequest(new
                {
                    error = "Invalid id"
                });
            }
            this._logger.LogDebug($"Retrieving connection: id {id}");
            var client = this._hub.GetClient(clientId);
            if (client == null)
            {
                return NotFound();
            }
            return ListResult.New(await client.GetMessageLogAsync());
        }
    }
}
