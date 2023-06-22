using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Queries;
using Transfer = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Controllers
{
    /// <summary>
    /// Controller for working with synchronization requests.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class SynchronizationController
        : ControllerBase
    {
        private readonly ILogger<SynchronizationController> _logger;
        private readonly IExecutionEngine executionEngine;
        private readonly Dictionary<string, Func<Task<DateTime>>> subSystems = new();

        /// <summary>
        /// Initialize a new <see cref="SynchronizationController"/> instance.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="executionEngine">The execution engine for processing commands and queries.</param>
        public SynchronizationController(ILogger<SynchronizationController> logger, IExecutionEngine executionEngine)
        {
            this._logger = logger;
            this.executionEngine = executionEngine;

            this.subSystems.Add("robot", RetrieveLastRobotUpdate);
            this.subSystems.Add("robotlog", RetrieveLastRobotLogUpdate);
            this.subSystems.Add("robottype", RetrieveLastRobotTypeUpdate);
        }

        /// <summary>
        /// Retrieves the synchronization status of this system.
        /// </summary>
        /// <param name="system">The subsystem to query.</param>
        /// <returns>A <see cref="ListResult{TData}"/> instance containing details..</returns>
        [HttpGet]
        [HttpGet("{system}")]
        public async Task<ActionResult<ListResult<Transfer.SynchronizationStatus>>> Get(string? system = null)
        {
            var results = new List<Transfer.SynchronizationStatus>();
            if (string.IsNullOrEmpty(system))
            {
                this._logger.LogInformation("Getting system synchronization status");
                foreach (var subSystem in this.subSystems)
                {
                    var status = new Transfer.SynchronizationStatus
                    {
                        Name = subSystem.Key,
                        WhenLastUpdated = await subSystem.Value(),
                    };
                    results.Add(status);
                }
            }
            else
            {
                var systemName = system.ToLowerInvariant();
                if (!this.subSystems.TryGetValue(systemName, out var subSystem))
                {
                    return NotFound();
                }

                this._logger.LogInformation("Getting synchronization status for {system}", system);
                var status = new Transfer.SynchronizationStatus
                {
                    Name = systemName,
                    WhenLastUpdated = await subSystem(),
                };
                results.Add(status);
            }

            return ListResult.New(results);
        }

        private async Task<DateTime> RetrieveLastRobotLogUpdate()
        {
            var value = await this.executionEngine
                .Query<RobotData>()
                .RetrieveLastLogAsync();
            return value?.WhenLastUpdated ?? DateTime.MinValue;
        }

        private async Task<DateTime> RetrieveLastRobotTypeUpdate()
        {
            var value = await this.executionEngine
                .Query<RobotTypeData>()
                .RetrieveLastUpdatedAsync();
            return value?.WhenLastUpdated ?? DateTime.MinValue;
        }

        private async Task<DateTime> RetrieveLastRobotUpdate()
        {
            var value = await this.executionEngine
                .Query<RobotData>()
                .RetrieveLastUpdatedAsync();
            return value?.WhenLastUpdated ?? DateTime.MinValue;
        }
    }
}