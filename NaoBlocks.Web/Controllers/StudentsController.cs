﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NaoBlocks.Core.Commands;
using NaoBlocks.Core.Models;
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

        [HttpDelete("{id}")]
        public async Task<ActionResult<Dtos.ExecutionResult>> DeleteStudent(string id)
        {
            this._logger.LogInformation($"Deleting student '{id}'");
            var command = new DeleteUserCommand
            {
                Name = id
            };
            return await this.commandManager.ExecuteForHttp(command);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Dtos.Student>> GetStudent(string id)
        {
            this._logger.LogDebug($"Retrieving student: id {id}");
            var student = await this.session.Query<User>()
                                            .Where(u => u.Name == id && u.Role == UserRole.Student)
                                            .FirstOrDefaultAsync();
            if (student == null)
            {
                return NotFound();
            }

            this._logger.LogDebug("Retrieved student");
            return new Dtos.Student
            {
                Name = student.Name
            };
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
            var command = new AddUserCommand
            {
                Name = student.Name
            };
            return await this.commandManager.ExecuteForHttp(command);
        }
    }
}