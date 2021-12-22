using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Helpers;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Data = NaoBlocks.Engine.Data;
using Transfer = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Controllers
{
    /// <summary>
    /// A controller for working with programs.
    /// </summary>
    [Route("api/v1/users/{user}/[controller]")]
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class ProgramsController : ControllerBase
    {
        private readonly ILogger<ProgramsController> _logger;
        private readonly IExecutionEngine executionEngine;

        /// <summary>
        /// Initialises a new <see cref="ProgramsController"/> instance.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="executionEngine">The execution engine for processing commands and queries.</param>
        public ProgramsController(ILogger<ProgramsController> logger, IExecutionEngine executionEngine)
        {
            this._logger = logger;
            this.executionEngine = executionEngine;
        }

        /// <summary>
        /// Deletes a program.
        /// </summary>
        /// <param name="user">The name of the user.</param>
        /// <param name="number">The program number to delete.</param>
        /// <returns>The result of execution.</returns>
        [HttpDelete("{number}")]
        [Authorize(Policy = "Administrator")]
        public async Task<ActionResult<ExecutionResult>> Delete(string number, string? user)
        {
            this._logger.LogInformation($"Deleting program #{number} from {user}");

            if (!long.TryParse(number, out var programNumber))
            {
                return BadRequest(new
                {
                    error = "Invalid program number"
                });
            }

            var command = new DeleteProgram
            {
                ProgramNumber = programNumber,
                UserName = user
            };
            return await this.executionEngine.ExecuteForHttp(command);
        }

        /// <summary>
        /// Retrieves a program for a user.
        /// </summary>
        /// <param name="number">The program number.</param>
        /// <param name="user">The name of the user.</param>
        /// <returns>A <see cref="Dtos.CodeProgram"/> if found, a 404 error otherwise.</returns>
        [HttpGet("{number}")]
        public async Task<ActionResult<Dtos.CodeProgram>> Get(string number, string? user)
        {
            var currentUser = await this.LoadUserAsync(this.executionEngine);
            if (currentUser == null) return Unauthorized();

            if (!long.TryParse(number, out var programNumber))
            {
                return BadRequest(new
                {
                    error = "Invalid program number"
                });
            }

            if ((currentUser.Role == Data.UserRole.Student) || string.IsNullOrEmpty(user)) user = currentUser.Name;
            this._logger.LogDebug($"Retrieving program {number} for {user}");

            if (user != currentUser.Name)
            {
                currentUser = await this.executionEngine
                    .Query<UserData>()
                    .RetrieveByNameAsync(user)
                    .ConfigureAwait(false);
                if (currentUser == null) return NotFound();
            }

            var program = await this.executionEngine
                .Query<CodeData>()
                .RetrieveCodeAsync(currentUser.Name, programNumber)
                .ConfigureAwait(false);
            if (program == null)
            {
                return NotFound();
            }

            this._logger.LogDebug("Retrieved program");
            return Dtos.CodeProgram.FromModel(program!, true);
        }

        /// <summary>
        /// Retrieves all the programs for a user, with an optional filter.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="size">The number of programs per page.</param>
        /// <param name="user">The name of the user.</param>
        /// <param name="type">An optional filter for the program type.</param>
        /// <returns>A page of <see cref="Dtos.CodeProgram"/> instances.</returns>
        [HttpGet()]
        public async Task<ActionResult<ListResult<Dtos.CodeProgram>>> List(int? page, int? size, string? user, string? type)
        {
            var currentUser = await this.LoadUserAsync(this.executionEngine);
            if (currentUser == null) return Unauthorized();

            if ((currentUser.Role == Data.UserRole.Student) || string.IsNullOrEmpty(user)) user = currentUser.Name;
            this._logger.LogDebug($"Retrieving programs for {user}");

            if (user != currentUser.Name)
            {
                currentUser = await this.executionEngine
                    .Query<UserData>()
                    .RetrieveByNameAsync(user)
                    .ConfigureAwait(false);
                if (currentUser == null) return NotFound();
            }

            (int pageNum, int pageSize) = this.ValidatePageArguments(page, size);
            this._logger.LogDebug($"Retrieving programs: page {pageNum} with size {pageSize}");
            var namedOnly = "all".Equals(type, StringComparison.InvariantCultureIgnoreCase);
            var allPrograms = await this.executionEngine
                .Query<CodeData>()
                .RetrieveForUserAsync(currentUser.Name, pageNum, pageSize, namedOnly)
                .ConfigureAwait(false);
            var count = allPrograms.Items?.Count() ?? 0;
            this._logger.LogDebug($"Retrieved {count} programs");
            var result = new ListResult<Dtos.CodeProgram>
            {
                Count = allPrograms.Count,
                Page = pageNum,
                Items = allPrograms.Items?.Select(p => Dtos.CodeProgram.FromModel(p))
            };
            return result;
        }

        /// <summary>
        /// Adds a program for a user.
        /// </summary>
        /// <param name="program">The program to add.</param>
        /// <param name="user">The name of the user.</param>
        /// <returns>The result of the execution.</returns>
        [HttpPost()]
        public async Task<ActionResult<ExecutionResult<Dtos.CodeProgram>>> Post(Dtos.CodeProgram? program, string? user)
        {
            var currentUser = await this.LoadUserAsync(this.executionEngine);
            if (currentUser == null) return Unauthorized();

            if ((currentUser.Role == Data.UserRole.Student) || string.IsNullOrEmpty(user)) user = currentUser.Name;
            this._logger.LogDebug($"Retrieving programs for {user}");

            if (user != currentUser.Name)
            {
                currentUser = await this.executionEngine
                    .Query<UserData>()
                    .RetrieveByNameAsync(user)
                    .ConfigureAwait(false);
                if (currentUser == null) return NotFound();
            }

            if (program == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing program details"
                });
            }

            this._logger.LogInformation($"Adding new program '{program.Name}'");
            var command = new StoreProgram
            {
                Name = program.Name,
                Code = program.Code,
                UserName = currentUser.Name,
                RequireName = true
            };
            return await this.executionEngine.ExecuteForHttp<Data.CodeProgram, Dtos.CodeProgram>
                (command, p => Dtos.CodeProgram.FromModel(p!));
        }
    }
}
