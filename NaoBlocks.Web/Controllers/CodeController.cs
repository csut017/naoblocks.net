using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NaoBlocks.Core.Models;
using NaoBlocks.Web.Helpers;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Commands = NaoBlocks.Core.Commands;

namespace NaoBlocks.Web.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class CodeController : ControllerBase
    {
        private readonly ILogger<CodeController> _logger;
        private readonly Commands.ICommandManager commandManager;
        private readonly IAsyncDocumentSession session;

        public CodeController(ILogger<CodeController> logger, Commands.ICommandManager commandManager, IAsyncDocumentSession session)
        {
            this._logger = logger;
            this.commandManager = commandManager;
            this.session = session;
        }

        [HttpPost("compile")]
        public async Task<ActionResult<Dtos.ExecutionResult<Dtos.CompiledCodeProgram?>>> Compile(Dtos.CodeProgram codeToCompile)
        {
            if (codeToCompile == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing code to compile"
                });
            }

            this._logger.LogInformation("Compiling code");
            var compileCommand = new Commands.CompileCode
            {
                Code = codeToCompile.Code
            };
            if (!codeToCompile.Store.GetValueOrDefault(false))
            {
                return await this.commandManager.ExecuteForHttp(
                    compileCommand,
                    Dtos.CompiledCodeProgram.FromModel);
            }

            var user = await this.LoadUser(this.session).ConfigureAwait(false);
            if (user == null) return NotFound();
            var storeCommand = new Commands.StoreProgram
            {
                UserId = user.Id,
                Code = codeToCompile.Code
            };

            var batchCommand = new Commands.Batch(compileCommand, storeCommand);
            return await this.commandManager.ExecuteForHttp(
                batchCommand,
                r =>
                {
                    var results = r.ToArray();
                    var output = Dtos.CompiledCodeProgram.FromModel(results[0].As<CompiledCodeProgram>().Output);
                    var store = results[1].As<CodeProgram>().Output;
                    if (output != null) output.ProgramId = store?.Number;

                    var opts = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        IgnoreNullValues = true,
                        WriteIndented = true
                    };
                    opts.Converters.Add(new JsonStringEnumConverter());
                    var jsonString = JsonSerializer.Serialize(output, opts);
                    this._logger.LogDebug(jsonString);

                    return output;
                });
        }

        [HttpGet("{user}/{program}")]
        [Authorize("Robot")]
        public async Task<ActionResult<Dtos.ExecutionResult<Dtos.CompiledCodeProgram?>>> Get(string user, long program)
        {
            this._logger.LogInformation($"Getting program {program} for {user}");
            var userDetails = await this.session.Query<User>()
                .FirstOrDefaultAsync(u => u.Name == user);
            if (userDetails == null) return NotFound();

            var programDetails = userDetails.Programs.FirstOrDefault(p => p.Number == program);
            if (programDetails == null) return NotFound();

            this._logger.LogInformation("Compiling code");
            var compileCommand = new Commands.CompileCode
            {
                Code = programDetails.Code
            };
            return await this.commandManager.ExecuteForHttp(
                compileCommand,
                Dtos.CompiledCodeProgram.FromModel);
        }
    }
}