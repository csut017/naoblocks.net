using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Web.Helpers;
using Commands = NaoBlocks.Engine.Commands;
using Data = NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.Controllers
{
    /// <summary>
    /// A controller for working with snapshots
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class SnapshotsController : ControllerBase
    {
        private readonly ILogger<SnapshotsController> logger;
        private readonly IExecutionEngine executionEngine;

        /// <summary>
        /// Initialises a new <see cref="SnapshotsController"/> instance.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="executionEngine">The execution engine for processing commands and queries.</param>
        public SnapshotsController(ILogger<SnapshotsController> logger, IExecutionEngine executionEngine)
        {
            this.logger = logger;
            this.executionEngine = executionEngine;
        }

        /// <summary>
        /// Adds a new snapshot.
        /// </summary>
        /// <param name="value">The snapshot to add.</param>
        /// <returns>The result of execution.</returns>
        [HttpPost]
        public async Task<ActionResult<ExecutionResult<Dtos.Snapshot>>> Post(Dtos.Snapshot? value)
        {
            if (value == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing snapshot details"
                });
            }

            var currentUser = await this.LoadUserAsync(this.executionEngine);
            if (currentUser == null) return Unauthorized();

            this.logger.LogInformation($"Adding new snapshot for '{currentUser.Name}'");
            var command = new Commands.StoreSnapshot
            {
                Source = value.Source,
                UserName = currentUser.Id,
                State = value.State
            };
            if (value.Values != null)
            {
                foreach (var subValue in value.Values)
                {
                    command.Values.Add(new Data.NamedValue
                    {
                        Name = subValue.Name,
                        Value = subValue.Value
                    });
                }
            }

            return await this.executionEngine.ExecuteForHttp<Data.Snapshot, Dtos.Snapshot>(
                command, s => Dtos.Snapshot.FromModel(s!));
        }
    }
}
