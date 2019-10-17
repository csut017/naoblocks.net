using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NaoBlocks.Web.Commands;
using NaoBlocks.Web.Dtos;
using NaoBlocks.Web.Helpers;
using System.Linq;
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
            var errors = await this.commandManager.ValidateAsync(command);
            if (errors.Any())
            {
                return this.BadRequest(new ExecutionResult<RobotCodeCompilation>
                {
                    ValidationErrors = errors
                });
            }

            var result = await this.commandManager.ApplyAsync(command);
            if (!result.WasSuccessful)
            {
                this._logger.LogInformation("Code compilation failed: " + result.Error);
                return this.StatusCode(StatusCodes.Status500InternalServerError, new ExecutionResult<RobotCodeCompilation>
                {
                    ExecutionErrors = new[] { result.Error }
                });
            }

            await this.commandManager.CommitAsync();
            var errorCount = command.Output?.Errors?.Count() ?? 0;
            this._logger.LogInformation($"Code compiled with {errorCount} error(s)");
            return ExecutionResult.New(command.Output);
        }
    }
}