using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NaoBlocks.Common;
using NaoBlocks.Core.Models;
using NaoBlocks.Web.Helpers;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Linq;
using System.Threading.Tasks;

using Commands = NaoBlocks.Core.Commands;
using Generators = NaoBlocks.Core.Generators;

namespace NaoBlocks.Web.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Policy = "Administrator")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly Commands.ICommandManager commandManager;
        private readonly IAsyncDocumentSession session;

        public UsersController(ILogger<UsersController> logger, Commands.ICommandManager commandManager, IAsyncDocumentSession session)
        {
            this._logger = logger;
            this.commandManager = commandManager;
            this.session = session;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ExecutionResult>> Delete(string id)
        {
            this._logger.LogInformation($"Deleting user '{id}'");
            var command = new Commands.DeleteUser
            {
                Name = id
            };
            return await this.commandManager.ExecuteForHttp(command);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Dtos.User>> GetUser(string id)
        {
            this._logger.LogDebug($"Retrieving user: id {id}");
            var user = await this.session.Query<User>()
                                            .FirstOrDefaultAsync(u => u.Name == id);
            if (user == null)
            {
                return NotFound();
            }

            this._logger.LogDebug("Retrieved user");
            return Dtos.User.FromModel(user, true);
        }

        [HttpGet]
        public async Task<ListResult<Dtos.User>> GetUsers(int? page, int? size)
        {
            var pageSize = size ?? 25;
            var pageNum = page ?? 0;
            if (pageSize > 100) pageSize = 100;

            this._logger.LogDebug($"Retrieving users: page {pageNum} with size {pageSize}");
            var users = await this.session.Query<User>()
                                            .Statistics(out QueryStatistics stats)
                                            .OrderBy(s => s.Name)
                                            .Skip(pageNum * pageSize)
                                            .Take(pageSize)
                                            .ToListAsync();
            var count = users.Count;
            this._logger.LogDebug($"Retrieved {count} users");
            var result = new ListResult<Dtos.User>
            {
                Count = stats.TotalResults,
                Page = pageNum,
                Items = users.Select(s => Dtos.User.FromModel(s))
            };
            return result;
        }

        [HttpPost]
        public async Task<ActionResult<ExecutionResult<Dtos.User>>> Post(Dtos.User user)
        {
            if (user == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing user details"
                });
            }

            this._logger.LogInformation($"Adding new user '{user.Name}'");
            if (!Enum.TryParse<UserRole>(user.Role, out UserRole role))
            {
                role = UserRole.Unknown;
            }

            var command = new Commands.AddUser
            {
                Name = user.Name,
                Password = user.Password,
                Role = role
            };
            return await this.commandManager.ExecuteForHttp(command, s => Dtos.User.FromModel(s));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ExecutionResult>> Put(string? id, Dtos.User? user)
        {
            if ((user == null) || string.IsNullOrEmpty(id))
            {
                return this.BadRequest(new
                {
                    Error = "Missing user details"
                });
            }

            this._logger.LogInformation($"Updating user '{id}'");
            if (!Enum.TryParse<UserRole>(user.Role, out UserRole role))
            {
                role = UserRole.Unknown;
            }

            var command = new Commands.UpdateUser
            {
                CurrentName = id,
                Name = user.Name,
                Password = user.Password,
                Role = role
            };
            return await this.commandManager.ExecuteForHttp(command);
        }

        //[HttpGet("export/list")]
        //public async Task<ActionResult> ExportList()
        //{
        //    var excelData = await Generators.UsersList.GenerateAsync(this.session);
        //    var contentType = ContentTypes.Xlsx;
        //    var fileName = "Users-List.xlsx";
        //    return File(excelData, contentType, fileName);
        //}
    }
}