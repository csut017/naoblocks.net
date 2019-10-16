using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NaoBlocks.Parser;
using NaoBlocks.Web.Commands;
using NaoBlocks.Web.Dtos;
using NaoBlocks.Web.Helpers;

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
            this._logger.LogInformation("Compiling code");
            var command = new CompileCodeCommand();
            var errors = await this.commandManager.ValidateAsync(command);
            if (errors.Any())
            {
                return new ExecutionResult<RobotCodeCompilation>
                {
                    ValidationErrors = errors
                };
            }

            var result = await this.commandManager.ApplyAsync(command);
            if (!result.WasSuccessful)
            {
                this._logger.LogInformation("Code compilation failed: " + result.Error);
                return new ExecutionResult<RobotCodeCompilation>
                {
                    ValidationErrors = errors
                };
            }

            await this.commandManager.CommitAsync();
            this._logger.LogInformation("Code compiled with " + command.Output.Errors.Count().ToString() + " error(s)");
            return new ExecutionResult<RobotCodeCompilation>
            {
                Output = command.Output
            };
        }
    }
}