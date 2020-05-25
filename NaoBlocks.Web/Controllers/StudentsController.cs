using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NaoBlocks.Core.Models;
using NaoBlocks.Web.Helpers;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System.Linq;
using System.Threading.Tasks;

using Commands = NaoBlocks.Core.Commands;
using Generators = NaoBlocks.Core.Generators;

namespace NaoBlocks.Web.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Policy = "Teacher")]
    public class StudentsController : ControllerBase
    {
        private readonly ILogger<StudentsController> _logger;
        private readonly Commands.ICommandManager commandManager;
        private readonly IAsyncDocumentSession session;

        public StudentsController(ILogger<StudentsController> logger, Commands.ICommandManager commandManager, IAsyncDocumentSession session)
        {
            this._logger = logger;
            this.commandManager = commandManager;
            this.session = session;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Dtos.ExecutionResult>> Delete(string id)
        {
            this._logger.LogInformation($"Deleting student '{id}'");
            var command = new Commands.DeleteUser
            {
                Name = id,
                Role = UserRole.Student
            };
            return await this.commandManager.ExecuteForHttp(command);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Dtos.Student>> GetStudent(string id)
        {
            this._logger.LogDebug($"Retrieving student: id {id}");
            var student = await this.session.Query<User>()
                                            .FirstOrDefaultAsync(u => u.Name == id && u.Role == UserRole.Student);
            if (student == null)
            {
                return NotFound();
            }

            this._logger.LogDebug("Retrieved student");
            return Dtos.Student.FromModel(student, true);
        }

        [HttpGet]
        public async Task<Dtos.ListResult<Dtos.Student>> GetStudents(int? page, int? size)
        {
            var pageSize = size ?? 25;
            var pageNum = page ?? 0;
            if (pageSize > 100) pageSize = 100;

            this._logger.LogDebug($"Retrieving students: page {pageNum} with size {pageSize}");
            var students = await this.session.Query<User>()
                                             .Statistics(out QueryStatistics stats)
                                             .Where(s => s.Role == UserRole.Student)
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
                Items = students.Select(s => Dtos.Student.FromModel(s))
            };
            return result;
        }

        [HttpPost]
        public async Task<ActionResult<Dtos.ExecutionResult<Dtos.Student>>> Post(Dtos.Student student)
        {
            if (student == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing student details"
                });
            }

            this._logger.LogInformation($"Adding new student '{student.Name}'");
            var command = new Commands.AddUser
            {
                Name = student.Name,
                Password = student.Password,
                Role = UserRole.Student,
                Settings = student.Settings
            };
            return await this.commandManager.ExecuteForHttp(command, s => Dtos.Student.FromModel(s, true));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Dtos.ExecutionResult>> Put(string? id, Dtos.Student? student)
        {
            if ((student == null) || string.IsNullOrEmpty(id))
            {
                return this.BadRequest(new
                {
                    Error = "Missing student details"
                });
            }

            this._logger.LogInformation($"Updating student '{id}'");
            var command = new Commands.UpdateUser
            {
                CurrentName = id,
                Name = student.Name,
                Password = student.Password,
                Role = UserRole.Student,
                Settings = student.Settings
            };
            return await this.commandManager.ExecuteForHttp(command);
        }

        [HttpGet("export/list")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult> ExportList()
        {
            var excelData = await Generators.StudentsList.GenerateAsync(this.session);
            var contentType = ContentTypes.Xlsx;
            var fileName = "Students-List.xlsx";
            return File(excelData, contentType, fileName);
        }

        [HttpGet("{id}/export/details")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult> ExportDetails(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return this.BadRequest(new
                {
                    Error = "Missing student details"
                });
            }

            var student = await this.session.Query<User>()
                                .FirstOrDefaultAsync(u => u.Name == id && u.Role == UserRole.Student);
            if (student == null)
            {
                return NotFound();
            }

            this._logger.LogDebug($"Generating details for {student.Name}");
            var excelData = await Generators.StudentDetails.GenerateAsync(this.session, student);
            var contentType = ContentTypes.Xlsx;
            var fileName = "Students-List.xlsx";
            return File(excelData, contentType, fileName);
        }

        [HttpGet("{id}/export/logs")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult> ExportLogs(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return this.BadRequest(new
                {
                    Error = "Missing student details"
                });
            }

            var student = await this.session.Query<User>()
                                            .FirstOrDefaultAsync(u => u.Name == id && u.Role == UserRole.Student);
            if (student == null)
            {
                return NotFound();
            }

            this._logger.LogDebug($"Generating details for {student.Name}");
            var excelData = await Generators.StudentLogs.GenerateAsync(this.session, student);
            var contentType = ContentTypes.Xlsx;
            var fileName = "Students-List.xlsx";
            return File(excelData, contentType, fileName);
        }
    }
}