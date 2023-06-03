using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Authorization;
using NaoBlocks.Web.Helpers;
using Transfer = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Controllers
{
    /// <summary>
    /// Controller for working with synchronization requests.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [RequireSynchronization]
    [Produces("application/json")]
    public class SynchronizationController
        : ControllerBase
    {
        private readonly IExecutionEngine executionEngine;
        private readonly ILogger<SynchronizationController> logger;
        private readonly Dictionary<string, Func<Task<DateTime>>> subSystems = new();

        /// <summary>
        /// Initialize a new <see cref="SynchronizationController"/> instance.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="executionEngine">The execution engine for processing commands and queries.</param>
        public SynchronizationController(ILogger<SynchronizationController> logger, IExecutionEngine executionEngine)
        {
            this.logger = logger;
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
                this.logger.LogInformation("Getting system synchronization status");
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

                this.logger.LogInformation("Getting synchronization status for {system}", system);
                var status = new Transfer.SynchronizationStatus
                {
                    Name = systemName,
                    WhenLastUpdated = await subSystem(),
                };
                results.Add(status);
            }

            return ListResult.New(results);
        }

        /// <summary>
        /// Retrieves the changes to a system since a point in time.
        /// </summary>
        /// <param name="system">The subsystem to query.</param>
        /// <param name="from">The start date to retrieve logs from.</param>
        /// <param name="to">An optional to date to stop retrieving logs.</param>
        /// <param name="page">The page number.</param>
        /// <param name="size">The number of records.</param>
        /// <returns>A <see cref="ListResult{TData}"/> instance containing details.</returns>
        [HttpGet("{system}/logs/{from}")]
        public async Task<ActionResult<ListResult<string>>> ListChanges(string system, DateTime from, DateTime? to = null, int? page = null, int? size = null)
        {
            if (!Enum.TryParse<CommandTarget>(system, true, out var target))
            {
                return BadRequest(new
                {
                    error = $"System '{system}' is not a valid sub-system"
                });
            }

            (int pageNum, int pageSize) = this.ValidatePageArguments(page, size);
            this.logger.LogDebug("Retrieving change: page {pageNum} with size {pageSize}", pageNum, pageSize);

            to ??= DateTime.MaxValue;
            if ((to.Value - from).TotalDays > 90) to = from.AddDays(90);
            var results = new List<string>();
            await foreach (var log in executionEngine.ListDehydratedCommandLogsAsync(from, to.Value, target))
            {
                results.Add(log);
            }

            return ListResult.New(results.Skip(pageNum * pageSize).Take(pageSize), results.Count, pageNum);
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