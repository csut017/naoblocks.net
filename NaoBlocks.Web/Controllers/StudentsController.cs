using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NaoBlocks.Web.Commands;
using NaoBlocks.Web.Helpers;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly ILogger<StudentsController> _logger;
        private readonly ICommandManager commandManager;
        private readonly IAsyncDocumentSession session;

        public StudentsController(ILogger<StudentsController> logger, ICommandManager commandManager, IAsyncDocumentSession session)
        {
            this._logger = logger;
            this.commandManager = commandManager;
            this.session = session;
        }

        [HttpGet]
        public async Task<Dtos.ListResult<Dtos.Student>> Get(int? page, int? size)
        {
            var pageSize = size ?? 25;
            var pageNum = page ?? 0;
            if (pageSize > 100) pageSize = 100;

            this._logger.LogDebug($"Retrieving students: page {pageNum} with size {pageSize}");
            var students = await this.session.Query<Models.User>()
                .Statistics(out QueryStatistics stats)
                .Where(s => s.Role == Models.UserRole.Student)
                .OrderBy(s => s.Name)
                .Skip(pageNum * pageSize)
                .Take(pageSize)
                .ToListAsync();
            var count = students.Count;
            this._logger.LogDebug($"Retrieved {count} students");
            var result = new Dtos.ListResult<Dtos.Student>
            {
                Count = stats.TotalResults,
                Page = pageNum,
                Items = students.Select(s => new Dtos.Student { Name = s.Name })
            };
            return result;
        }

        [HttpPost]
        public async Task<ActionResult<Dtos.ExecutionResult>> Post(Dtos.Student student)
        {
            if (student == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing student details"
                });
            }

            this._logger.LogInformation($"Adding new student '{student.Name}'");
            var command = new AddStudentComment
            {
                Name = student.Name
            };
            var errors = await this.commandManager.ValidateAsync(command);
            if (errors.Any())
            {
                return this.BadRequest(new Dtos.ExecutionResult<Dtos.Student>
                {
                    ValidationErrors = errors
                });
            }

            var result = await this.commandManager.ApplyAsync(command);
            if (!result.WasSuccessful)
            {
                this._logger.LogInformation("Code compilation failed: " + result.Error);
                return this.StatusCode(StatusCodes.Status500InternalServerError, new Dtos.ExecutionResult<Dtos.Student>
                {
                    ExecutionErrors = new[] { result.Error }
                });
            }

            await this.commandManager.CommitAsync();
            this._logger.LogInformation($"Student '{student.Name}' added");
            return new Dtos.ExecutionResult();
        }
    }
}