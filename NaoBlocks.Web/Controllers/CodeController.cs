using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NaoBlocks.Parser;
using NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CodeController : ControllerBase
    {
        private readonly ILogger<CodeController> _logger;

        public CodeController(ILogger<CodeController> logger)
        {
            this._logger = logger;
        }

        [HttpPost("compile")]
        public async Task<ActionResult<ParseResult>> Compile(CodeCompileRequest request)
        {
            this._logger.LogInformation("Compiling code");
            var parser = CodeParser.New(request.Code);
            var result = await parser.ParseAsync();
            this._logger.LogInformation("Code compiled with " + result.Errors.Count().ToString() + " error(s)");
            return result;
        }
    }
}