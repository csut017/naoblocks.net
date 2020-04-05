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
    [Authorize]
    public class ProgramsController : ControllerBase
    {
        private readonly ILogger<ProgramsController> _logger;
        private readonly ICommandManager commandManager;
        private readonly IAsyncDocumentSession session;

        public ProgramsController(ILogger<ProgramsController> logger, ICommandManager commandManager, IAsyncDocumentSession session)
        {
            this._logger = logger;
            this.commandManager = commandManager;
            this.session = session;
        }

        //[HttpDelete("{id}")]
        //public async Task<ActionResult<Dtos.ExecutionResult>> Delete(string id, string? user)
        //{
        //    this._logger.LogInformation($"Deleting program '{id}'");
        //    var command = new DeleteProgramCommand
        //    {
        //        MachineName = id
        //    };
        //    return await this.commandManager.ExecuteForHttp(command);
        //}

        [HttpGet("{id}")]
        public async Task<ActionResult<Dtos.CodeProgram>> GetProgram(string id, string? user)
        {
            var currentUser = await this.LoadUser(session);
            if (currentUser == null) return Unauthorized();

            if ((currentUser.Role == UserRole.Student) || string.IsNullOrEmpty(user)) user = currentUser.Name;
            this._logger.LogDebug($"Retrieving program {id} for {user}");

            if (user != currentUser.Name) currentUser = await session.Query<User>().FirstOrDefaultAsync(u => u.Name == user).ConfigureAwait(false);
            if (currentUser == null) return NotFound();

            var program = currentUser.Programs.FirstOrDefault(p => p.Name == id);
            if (program == null)
            {
                return NotFound();
            }

            this._logger.LogDebug("Retrieved program");
            return Dtos.CodeProgram.FromModelWithDetails(program);
        }

        [HttpGet]
        public async Task<ActionResult<Dtos.ListResult<Dtos.CodeProgram>>> GetPrograms(int? page, int? size, string? user, string? type)
        {
            var currentUser = await this.LoadUser(session);
            if (currentUser == null) return Unauthorized();

            if ((currentUser.Role == UserRole.Student) || string.IsNullOrEmpty(user)) user = currentUser.Name;
            this._logger.LogDebug($"Retrieving programs for {user}");

            if (user != currentUser.Name) currentUser = await session.Query<User>().FirstOrDefaultAsync(u => u.Name == user).ConfigureAwait(false);
            if (currentUser == null) return NotFound();

            var pageSize = size ?? 25;
            var pageNum = page ?? 0;
            if (pageSize > 100) pageSize = 100;

            this._logger.LogDebug($"Retrieving programs: page {pageNum} with size {pageSize}");
            var allPrograms = currentUser.Programs.AsQueryable();
            if (type != "all") allPrograms = allPrograms.Where(p => p.Name != null);
            var programs = allPrograms.OrderBy(s => s.Name)
                .Skip(pageNum * pageSize)
                .Take(pageSize)
                .ToList();
            var count = programs.Count;
            this._logger.LogDebug($"Retrieved {count} programs");
            var result = new Dtos.ListResult<Dtos.CodeProgram>
            {
                Count = allPrograms.Count(),
                Page = pageNum,
                Items = programs.Select(Dtos.CodeProgram.FromModel).Where(p => p != null)
            };
            return result;
        }

        [HttpPost]
        public async Task<ActionResult<Dtos.ExecutionResult<Dtos.CodeProgram>>> Post(Dtos.CodeProgram program, string? user)
        {
            var currentUser = await this.LoadUser(session);
            if (currentUser == null) return Unauthorized();

            if ((currentUser.Role == UserRole.Student) || string.IsNullOrEmpty(user)) user = currentUser.Name;
            this._logger.LogDebug($"Retrieving programs for {user}");

            if (user != currentUser.Name) currentUser = await session.Query<User>().FirstOrDefaultAsync(u => u.Name == user).ConfigureAwait(false);
            if (currentUser == null) return NotFound();

            if (program == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing program details"
                });
            }

            this._logger.LogInformation($"Adding new program '{program.Name}'");
            var command = new StoreProgramCommand
            {
                Name = program.Name,
                Code = program.Code,
                User = currentUser,
                RequireName = true
            };
            return await this.commandManager.ExecuteForHttp(command, Dtos.CodeProgram.FromModel);
        }
    }
}