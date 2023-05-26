using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Authorization;
using NaoBlocks.Web.Helpers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Data = NaoBlocks.Engine.Data;
using Transfer = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Controllers
{
    /// <summary>
    /// Controller for working with code.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class CodeController : ControllerBase
    {
        private readonly ILogger<CodeController> _logger;
        private readonly IExecutionEngine executionEngine;

        /// <summary>
        /// Initialise a new <see cref="CodeController"/> instance.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="executionEngine">The execution engine for processing commands and queries.</param>
        public CodeController(ILogger<CodeController> logger, IExecutionEngine executionEngine)
        {
            this._logger = logger;
            this.executionEngine = executionEngine;
        }

        /// <summary>
        /// Compiles a program.
        /// </summary>
        /// <param name="codeToCompile">The program to compile.</param>
        /// <returns>The result of compilation.</returns>
        [HttpPost("compile")]
        public async Task<ActionResult<ExecutionResult<Transfer.CompiledCodeProgram?>>> Compile(Transfer.CodeProgram codeToCompile)
        {
            this._logger.LogInformation("Compiling code");
            var compileCommand = new CompileCode
            {
                Code = codeToCompile.Code
            };
            if (!codeToCompile.Store.GetValueOrDefault(false))
            {
                return await this.executionEngine.ExecuteForHttp<Data.CompiledCodeProgram, Transfer.CompiledCodeProgram?>(
                    compileCommand,
                    Transfer.CompiledCodeProgram.FromModel);
            }

            var user = await this.LoadUserAsync(this.executionEngine).ConfigureAwait(false);
            if (user == null) return NotFound();
            var storeCommand = new StoreProgram
            {
                UserName = user.Name,
                Code = codeToCompile.Code,
                Source = codeToCompile.Source,
                Version = ControllerHelpers.GetVersion(),
            };

            var batchCommand = new Batch(compileCommand, storeCommand);
            return await this.executionEngine.ExecuteForHttp<IEnumerable<CommandResult>, Transfer.CompiledCodeProgram?>(
                batchCommand,
                r =>
                {
                    var results = r!.ToArray();
                    var output = Transfer.CompiledCodeProgram.FromModel(results[0].As<Data.CompiledCodeProgram>().Output);
                    var store = results[1].As<Data.CodeProgram>().Output;
                    if (output != null) output.ProgramId = store?.Number;

                    var opts = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        WriteIndented = true
                    };
                    opts.Converters.Add(new JsonStringEnumConverter());
                    var jsonString = JsonSerializer.Serialize(output, opts);
                    this._logger.LogDebug(jsonString);

                    return output;
                });
        }

        /// <summary>
        /// Retrieves a stored program.
        /// </summary>
        /// <param name="user">The user to retrieve the program for.</param>
        /// <param name="program">The identifier of the program.</param>
        /// <returns>A <see cref="Transfer.CompiledCodeProgram"/> instance containing the compiled program.</returns>
        [HttpGet("{user}/{program}")]
        [RequireTeacherOrRobot]
        public async Task<ActionResult<ExecutionResult<Transfer.CompiledCodeProgram?>>> Get(string user, long program)
        {
            this._logger.LogInformation("Getting program {program} for {user}", program, user);
            var userDetails = await this.executionEngine
                .Query<UserData>()
                .RetrieveByNameAsync(user)
                .ConfigureAwait(false);

            if (userDetails == null) return NotFound();

            var programDetails = await this.executionEngine
                .Query<CodeData>()
                .RetrieveCodeAsync(userDetails.Id!, program)
                .ConfigureAwait(false);
            if (programDetails == null) return NotFound();

            this._logger.LogInformation("Compiling code");
            var compileCommand = new CompileCode
            {
                Code = programDetails.Code
            };
            return await this.executionEngine.ExecuteForHttp<Data.CompiledCodeProgram, Transfer.CompiledCodeProgram?>(
                compileCommand,
                Transfer.CompiledCodeProgram.FromModel);
        }
    }
}