using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Common;
using NaoBlocks.Web.Communications;
using NaoBlocks.Web.Dtos;
using NaoBlocks.Web.Helpers;

namespace NaoBlocks.Web.Controllers
{
    /// <summary>
    /// A controller for working with clients.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    [Authorize(Policy = "Teacher")]
    [Produces("application/json")]
    public class ClientsController : ControllerBase
    {
        private readonly IHub hub;
        private readonly ILogger<ClientsController> logger;

        /// <summary>
        /// Initialises a new <see cref="ClientsController"/> instance.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="hub">The <see cref="IHub"/> instance to use.</param>
        public ClientsController(ILogger<ClientsController> logger, IHub hub)
        {
            this.logger = logger;
            this.hub = hub;
        }

        /// <summary>
        /// Retrieves a page of current connections.
        /// </summary>
        /// <param name="type">The type of connection to list.</param>
        /// <param name="page">The page number.</param>
        /// <param name="size">The number of records.</param>
        /// <returns>A <see cref="ListResult{TData}"/> containing the connections.</returns>
        [HttpGet("{type}")]
        public ActionResult<ListResult<CommunicationsClient>> List(ClientConnectionType type, int? page, int? size)
        {
            (int pageNum, int pageSize) = this.ValidatePageArguments(page, size);

            this.logger.LogDebug($"Listing connections");
            return ListResult.New(this.hub
                .GetClients(type)
                .Where(c => c != null)
                .Select(CommunicationsClient.FromModel));
        }

        /// <summary>
        /// Retrieves a connection log by its id.
        /// </summary>
        /// <param name="id">The id of the connection.</param>
        /// <returns>Either a 404 (not found) or the connection details.</returns>
        [HttpGet("{id}/logs")]
        public async Task<ActionResult<ListResult<ClientMessage>>> GetLog(string id)
        {
            if (!long.TryParse(id, out long clientId))
            {
                return BadRequest(new
                {
                    error = "Invalid id"
                });
            }

            this.logger.LogDebug($"Retrieving connection: id {id}");
            var client = this.hub.GetClient(clientId);
            if (client == null)
            {
                return NotFound();
            }
            return ListResult.New(await client.GetMessageLogAsync());
        }
    }
}
