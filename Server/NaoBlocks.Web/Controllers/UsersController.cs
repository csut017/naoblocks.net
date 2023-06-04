﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Helpers;
using System.Text;
using System.Web;

using Commands = NaoBlocks.Engine.Commands;

using Data = NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.Controllers
{
    /// <summary>
    /// A controller for working with users.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Policy = "Administrator")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IExecutionEngine executionEngine;
        private readonly ILogger<UsersController> logger;

        /// <summary>
        /// Initialises a new <see cref="UsersController"/> instance.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="executionEngine">The execution engine for processing commands and queries.</param>
        public UsersController(ILogger<UsersController> logger, IExecutionEngine executionEngine)
        {
            this.logger = logger;
            this.executionEngine = executionEngine;
        }

        /// <summary>
        /// Deletes a user by their name.
        /// </summary>
        /// <param name="id">The name of the user.</param>
        /// <returns>The result of execution.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ExecutionResult>> Delete(string id)
        {
            this.logger.LogInformation($"Deleting user '{id}'");
            var command = new Commands.DeleteUser
            {
                Name = id
            };
            return await this.executionEngine.ExecuteForHttp(command);
        }

        /// <summary>
        /// Generates a login token for a user.
        /// </summary>
        /// <param name="name">The name of the user.</param>
        /// <returns>Either a 404 (not found) or the user details.</returns>
        [HttpGet("{name}/token")]
        public async Task<ActionResult<Data.MachineToken>> GenerateLoginToken(string name)
        {
            this.logger.LogInformation("Generating login token for {name}", name);
            var command = new GenerateLoginToken
            {
                Name = name,
            };

            var result = await this.executionEngine.ExecuteForHttp<Data.User>(command);
            if (result?.Value?.Successful != true)
            {
                return result?.Result ?? BadRequest();
            }

            var user = result!.Value!.Output!;
            var key = Convert.ToBase64String(Encoding.UTF8.GetBytes($"token:{user?.PlainPassword}"));
            var fullToken = HttpUtility.UrlEncode(key);
            var returnValue = new Data.MachineToken
            {
                Token = fullToken,
            };
            return returnValue;
        }

        /// <summary>
        /// Retrieves a user by their name.
        /// </summary>
        /// <param name="name">The name of the user.</param>
        /// <returns>Either a 404 (not found) or the user details.</returns>
        [HttpGet("{name}")]
        public async Task<ActionResult<Dtos.User>> Get(string name)
        {
            this.logger.LogDebug($"Retrieving user: {name}");
            var user = await this.executionEngine
                .Query<UserData>()
                .RetrieveByNameAsync(name)
                .ConfigureAwait(false);
            if (user == null)
            {
                this.logger.LogDebug("User not found");
                return NotFound();
            }

            this.logger.LogDebug("Retrieved user");
            return Dtos.User.FromModel(user, Dtos.DetailsType.Standard);
        }

        /// <summary>
        /// Retrieves a page of users.
        /// </summary>
        /// <param name="page">The page number.</param>
        /// <param name="size">The number of records.</param>
        /// <returns>A <see cref="ListResult{TData}"/> containing the users.</returns>
        [HttpGet]
        public async Task<ListResult<Dtos.User>> List(int? page, int? size)
        {
            (int pageNum, int pageSize) = this.ValidatePageArguments(page, size);

            this.logger.LogDebug($"Retrieving users: page {pageNum} with size {pageSize}");

            var dataPage = await this.executionEngine
                .Query<UserData>()
                .RetrievePageAsync(pageNum, pageSize)
                .ConfigureAwait(false);
            var count = dataPage.Items?.Count() ?? 0;
            this.logger.LogDebug($"Retrieved {count} users");

            var result = new ListResult<Dtos.User>
            {
                Count = dataPage.Count,
                Page = dataPage.Page,
                Items = dataPage.Items?.Select(s => Dtos.User.FromModel(s))
            };
            return result;
        }

        /// <summary>
        /// Adds a new user.
        /// </summary>
        /// <param name="user">The user to add.</param>
        /// <returns>The result of execution.</returns>
        [HttpPost]
        public async Task<ActionResult<ExecutionResult<Dtos.User>>> Post(Dtos.User? user)
        {
            if (user == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing user details"
                });
            }

            this.logger.LogInformation($"Adding new user '{user.Name}'");
            if (!Enum.TryParse(user.Role, out Data.UserRole role))
            {
                role = Data.UserRole.Unknown;
            }

            var command = new Commands.AddUser
            {
                Name = user.Name,
                Password = user.Password,
                Role = role
            };
            return await this.executionEngine.ExecuteForHttp<Data.User, Dtos.User>(
                command, s => Dtos.User.FromModel(s!));
        }

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        /// <param name="id">The name of the user.</param>
        /// <param name="user">The updated details.</param>
        /// <returns>The result of execution.</returns>

        [HttpPut("{id}")]
        public async Task<ActionResult<ExecutionResult<Dtos.User>>> Put(string? id, Dtos.User? user)
        {
            if ((user == null) || string.IsNullOrEmpty(id))
            {
                return this.BadRequest(new
                {
                    Error = "Missing user details"
                });
            }

            this.logger.LogInformation($"Updating user '{id}'");
            if (!Enum.TryParse(user.Role, out Data.UserRole role))
            {
                role = Data.UserRole.Unknown;
            }

            var command = new Commands.UpdateUser
            {
                CurrentName = id,
                Name = user.Name,
                Password = user.Password,
                Role = role
            };
            return await this.executionEngine.ExecuteForHttp<Data.User, Dtos.User>(
                command, s => Dtos.User.FromModel(s!));
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