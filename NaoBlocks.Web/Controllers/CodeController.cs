using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NaoBlocks.Core.Commands;
using NaoBlocks.Core.Models;
using NaoBlocks.Web.Helpers;
using Raven.Client.Documents.Session;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class CodeController : ControllerBase
    {
        private readonly ILogger<CodeController> _logger;
        private readonly ICommandManager commandManager;
        private readonly IAsyncDocumentSession session;

        public CodeController(ILogger<CodeController> logger, ICommandManager commandManager, IAsyncDocumentSession session)
        {
            this._logger = logger;
            this.commandManager = commandManager;
            this.session = session;
        }

        [HttpPost("compile")]
        public async Task<ActionResult<Dtos.ExecutionResult<CompiledProgram>>> Compile(Dtos.CodeProgram codeToCompile)
        {
            if (codeToCompile == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing code to compile"
                });
            }

            this._logger.LogInformation("Compiling code");
            var compileCommand = new CompileCodeCommand
            {
                Code = codeToCompile.Code
            };
            if (!codeToCompile.Store.GetValueOrDefault(false))
            {
                return await this.commandManager.ExecuteForHttp(
                    compileCommand,
                    c => c);
            }

            var user = await this.LoadUser(this.session).ConfigureAwait(false);
            if (user == null) return NotFound();
            var storeCommand = new StoreProgramCommand
            {
                UserId = user.Id,
                Code = codeToCompile.Code
            };

            var batchCommand = new BatchCommand(compileCommand, storeCommand);
            return await this.commandManager.ExecuteForHttp(
                batchCommand,
                r =>
                {
                    var results = r.ToArray();
                    var output = results[0].As<CompiledProgram>().Output;
                    var store = results[1].As<CodeProgram>().Output;
                    if (output != null) output.ProgramId = store?.Number;
                    return output;
                });
        }
    }
}