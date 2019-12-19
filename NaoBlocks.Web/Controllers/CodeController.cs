using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NaoBlocks.Core.Commands;
using NaoBlocks.Core.Models;
using NaoBlocks.Web.Dtos;
using NaoBlocks.Web.Helpers;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CodeController : ControllerBase
    {
        private readonly ILogger<CodeController> _logger;
        private readonly ICommandManager commandManager;

        public CodeController(ILogger<CodeController> logger, ICommandManager commandManager)
        {
            this._logger = logger;
            this.commandManager = commandManager;
        }

        [HttpPost("compile")]
        public async Task<ActionResult<ExecutionResult<RobotCodeCompilation>>> Compile(RobotCode codeToCompile)
        {
            if (codeToCompile == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing code to compile"
                });
            }

            this._logger.LogInformation("Compiling code");
            var command = new CompileCodeCommand
            {
                Code = codeToCompile.Program
            };
            return await this.commandManager.ExecuteForHttp(
                command,
                c => c);
        }
    }
}