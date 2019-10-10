using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace NaoBlocks.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(ILogger<StudentsController> logger)
        {
            this._logger = logger;
        }

        [HttpGet]
        public IEnumerable<Dtos.User> Get()
        {
            this._logger.LogDebug("Retrieving students");
            return new[] {
                new Dtos.User{ Name = "Test person" }
            };
        }
    }
}