using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Authorization;
using NaoBlocks.Web.Helpers;
using Commands = NaoBlocks.Engine.Commands;

using Data = NaoBlocks.Engine.Data;

using Transfer = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Controllers
{
    /// <summary>
    /// Controller for working with synchronization requests.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
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
        /// Deletes a synchronization source by their name.
        /// </summary>
        /// <param name="id">The name of the synchronization source.</param>
        /// <returns>The result of execution.</returns>
        [HttpDelete("{id}")]
        [RequireAdministrator]
        public async Task<ActionResult<ExecutionResult>> Delete(string id)
        {
            this.logger.LogInformation("Deleting synchronization source '{id}'", id);
            var command = new Commands.DeleteSynchronizationSource
            {
                Name = id
            };
            return await this.executionEngine.ExecuteForHttp(command);
        }

        /// <summary>
        /// Retrieves a synchronization source by their name.
        /// </summary>
        /// <param name="name">The name of the synchronization source.</param>
        /// <returns>Either a 404 (not found) or the synchronization source details.</returns>
        [HttpGet("{name}")]
        [RequireAdministrator]
        public async Task<ActionResult<Transfer.SynchronizationSource>> Get(string name)
        {
            this.logger.LogDebug("Retrieving synchronization source: {name}", name);
            var source = await this.executionEngine
                .Query<SynchronizationData>()
                .RetrieveSourceByNameAsync(name)
                .ConfigureAwait(false);
            if (source == null)
            {
                this.logger.LogDebug("Synchronization source not found");
                return NotFound();
            }

            this.logger.LogDebug("Retrieved synchronization source");
            return Transfer.SynchronizationSource.FromModel(source, Transfer.DetailsType.Standard);
        }

        /// <summary>
        /// Retrieves the synchronization status of this system.
        /// </summary>
        /// <param name="system">The subsystem to query.</param>
        /// <returns>A <see cref="ListResult{TData}"/> instance containing details..</returns>
        [HttpGet("sub")]
        [HttpGet("sub/{system}")]
        [RequireSynchronization]
        public async Task<ActionResult<ListResult<Transfer.SynchronizationStatus>>> GetSystem(string? system = null)
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
        /// Retrieves a page of synchronization sources.
        /// </summary>
        /// <param name="page">The page number.</param>
        /// <param name="size">The number of records.</param>
        /// <returns>A <see cref="ListResult{TData}"/> containing the synchronization sources.</returns>
        [HttpGet]
        [RequireAdministrator]
        public async Task<ListResult<Transfer.SynchronizationSource>> List(int? page, int? size)
        {
            (int pageNum, int pageSize) = this.ValidatePageArguments(page, size);

            this.logger.LogDebug("Retrieving synchronization sources: page {pageNum} with size {pageSize}", pageNum, pageSize);

            var dataPage = await this.executionEngine
                .Query<SynchronizationData>()
                .RetrieveSourcePageAsync(pageNum, pageSize)
                .ConfigureAwait(false);
            var count = dataPage.Items?.Count() ?? 0;
            this.logger.LogDebug("Retrieved {count} synchronization sources", count);

            var result = new ListResult<Transfer.SynchronizationSource>
            {
                Count = dataPage.Count,
                Page = dataPage.Page,
                Items = dataPage.Items?.Select(s => Transfer.SynchronizationSource.FromModel(s))
            };
            return result;
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
        [HttpGet("sub/{system}/logs/{from}")]
        [RequireSynchronization]
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

        /// <summary>
        /// Adds a new synchronization source.
        /// </summary>
        /// <param name="source">The synchronization source to add.</param>
        /// <returns>The result of execution.</returns>
        [HttpPost]
        [RequireAdministrator]
        public async Task<ActionResult<ExecutionResult<Transfer.SynchronizationSource>>> Post(Transfer.SynchronizationSource? source)
        {
            if (source == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing synchronization source details"
                });
            }

            this.logger.LogInformation("Adding new synchronization source '{name}'", source.Name);
            var command = new Commands.AddSynchronizationSource
            {
                Name = source.Name,
                Address = source.Address,
            };
            return await this.executionEngine.ExecuteForHttp<Data.SynchronizationSource, Transfer.SynchronizationSource>(
                command, s => Transfer.SynchronizationSource.FromModel(s!));
        }

        /// <summary>
        /// Updates an existing synchronization source.
        /// </summary>
        /// <param name="id">The name of the synchronization source.</param>
        /// <param name="source">The updated details.</param>
        /// <returns>The result of execution.</returns>
        [HttpPut("{id}")]
        [RequireAdministrator]
        public async Task<ActionResult<ExecutionResult<Transfer.SynchronizationSource>>> Put(string? id, Transfer.SynchronizationSource? source)
        {
            if ((source == null) || string.IsNullOrEmpty(id))
            {
                return this.BadRequest(new
                {
                    Error = "Missing synchronization source details"
                });
            }

            this.logger.LogInformation("Updating synchronization source '{id}'", id);
            var command = new Commands.UpdateSynchronizationSource
            {
                CurrentName = id,
                Name = source.Name,
                Address = source.Address,
            };
            return await this.executionEngine.ExecuteForHttp<Data.SynchronizationSource, Transfer.SynchronizationSource>(
                command, s => Transfer.SynchronizationSource.FromModel(s!));
        }

        /// <summary>
        /// Stores a token for a synchronization source.
        /// </summary>
        /// <param name="id">The name of the synchronization source.</param>
        /// <param name="value">The value containing the token.</param>
        /// <returns>The result of execution.</returns>
        [HttpPut("{id}/token")]
        [RequireAdministrator]
        public async Task<ActionResult<ExecutionResult<Transfer.SynchronizationSource>>> StoreToken(string? id, MachineToken? value)
        {
            if ((value == null) || string.IsNullOrEmpty(id))
            {
                return this.BadRequest(new
                {
                    Error = "Missing token details"
                });
            }

            this.logger.LogInformation("Updating synchronization source '{id}'", id);
            var command = new Commands.StoreSynchronizationSourceToken
            {
                Name = id,
                Token = value.Token,
            };
            return await this.executionEngine.ExecuteForHttp<Data.SynchronizationSource, Transfer.SynchronizationSource>(
                command, s => Transfer.SynchronizationSource.FromModel(s!));
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