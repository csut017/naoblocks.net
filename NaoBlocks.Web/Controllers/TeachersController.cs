using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [Authorize(Policy = "DeviceOnly")]
    public class TeachersController : ControllerBase
    {
        private readonly ILogger<TeachersController> _logger;
        private readonly ICommandManager commandManager;
        private readonly IAsyncDocumentSession session;

        public TeachersController(ILogger<TeachersController> logger, ICommandManager commandManager, IAsyncDocumentSession session)
        {
            this._logger = logger;
            this.commandManager = commandManager;
            this.session = session;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Dtos.ExecutionResult>> Delete(string id)
        {
            this._logger.LogInformation($"Deleting teacher '{id}'");
            var command = new DeleteUserCommand
            {
                Name = id,
                Role = UserRole.Teacher
            };
            return await this.commandManager.ExecuteForHttp(command);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Dtos.Teacher>> GetTeacher(string id)
        {
            this._logger.LogDebug($"Retrieving teacher: id {id}");
            var teacher = await this.session.Query<User>()
                                            .FirstOrDefaultAsync(u => u.Name == id && u.Role == UserRole.Teacher);
            if (teacher == null)
            {
                return NotFound();
            }

            this._logger.LogDebug("Retrieved teacher");
            return Dtos.Teacher.FromModel(teacher);
        }

        [HttpGet]
        public async Task<Dtos.ListResult<Dtos.Teacher>> GetTeachers(int? page, int? size)
        {
            var pageSize = size ?? 25;
            var pageNum = page ?? 0;
            if (pageSize > 100) pageSize = 100;

            this._logger.LogDebug($"Retrieving teachers: page {pageNum} with size {pageSize}");
            var teachers = await this.session.Query<User>()
                                             .Statistics(out QueryStatistics stats)
                                             .Where(s => s.Role == UserRole.Teacher)
                                             .OrderBy(s => s.Name)
                                             .Skip(pageNum * pageSize)
                                             .Take(pageSize)
                                             .ToListAsync();
            var count = teachers.Count;
            this._logger.LogDebug($"Retrieved {count} teachers");
            var result = new Dtos.ListResult<Dtos.Teacher>
            {
                Count = stats.TotalResults,
                Page = pageNum,
                Items = teachers.Select(Dtos.Teacher.FromModel)
            };
            return result;
        }

        [HttpPost]
        public async Task<ActionResult<Dtos.ExecutionResult<Dtos.Teacher>>> Post(Dtos.Teacher teacher)
        {
            if (teacher == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing teacher details"
                });
            }

            this._logger.LogInformation($"Adding new teacher '{teacher.Name}'");
            var command = new AddUserCommand
            {
                Name = teacher.Name,
                Password = teacher.Password,
                Role = UserRole.Teacher
            };
            return await this.commandManager.ExecuteForHttp(command, Dtos.Teacher.FromModel);
        }
    }
}