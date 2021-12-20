using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Helpers;
using Commands = NaoBlocks.Engine.Commands;
using Data = NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.Controllers
{
    /// <summary>
    /// A controller for working with teachers.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Policy = "Teacher")]
    public class TeachersController : ControllerBase
    {
        private readonly ILogger<TeachersController> logger;
        private readonly IExecutionEngine executionEngine;

        /// <summary>
        /// Initialises a new <see cref="TeachersController"/> instance.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="engine">The <see cref="IExecutionEngine"/> to use.</param>
        public TeachersController(ILogger<TeachersController> logger, IExecutionEngine engine)
        {
            this.logger = logger;
            this.executionEngine = engine;
        }

        /// <summary>
        /// Deletes a teacher.
        /// </summary>
        /// <param name="id">The id of the teacher to delete.</param>
        /// <returns>The result of execution.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ExecutionResult>> Delete(string id)
        {
            this.logger.LogInformation($"Deleting teacher '{id}'");
            var command = new Commands.DeleteUser
            {
                Name = id,
                Role = Data.UserRole.Teacher
            };
            return await this.executionEngine.ExecuteForHttp(command);
        }

        /// <summary>
        /// Retrieves the details on a teacher
        /// </summary>
        /// <param name="name">The name of the teacher.</param>
        [HttpGet("{name}")]
        public async Task<ActionResult<Dtos.Teacher>> GetTeacher(string name)
        {
            this.logger.LogDebug($"Retrieving teacher: {name}");
            var teacher = await this.executionEngine
                .Query<UserData>()
                .RetrieveByNameAsync(name)
                .ConfigureAwait(false);
            if ((teacher == null) || (teacher.Role != Data.UserRole.Teacher))
            {
                this.logger.LogDebug("Teacher not found");
                return NotFound();
            }

            this.logger.LogDebug("Retrieved teacher");
            return Dtos.Teacher.FromModel(teacher);
        }

        /// <summary>
        /// Retrieves a page of teachers.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="size">The number of items to retrieve in the page.</param>
        [HttpGet]
        public async Task<ListResult<Dtos.Teacher>> GetTeachers(int? page, int? size)
        {
            var pageSize = size ?? 25;
            var pageNum = page ?? 0;
            if (pageSize > 100) pageSize = 100;

            this.logger.LogDebug($"Retrieving teachers: page {pageNum} with size {pageSize}");
            var dataPage = await this.executionEngine
                .Query<UserData>()
                .RetrievePageAsync(pageNum, pageSize, Data.UserRole.Teacher)
                .ConfigureAwait(false);
            var count = dataPage.Items?.Count() ?? 0;
            this.logger.LogDebug($"Retrieved {count} teachers");
            var result = new ListResult<Dtos.Teacher>
            {
                Count = dataPage.Count,
                Page = pageNum,
                Items = dataPage.Items?.Select(Dtos.Teacher.FromModel)
            };
            return result;
        }

        /// <summary>
        /// Adds a new teacher.
        /// </summary>
        /// <param name="teacher">The details of the teacher.</param>
        /// <returns>The result of execution.</returns>
        [HttpPost]
        public async Task<ActionResult<ExecutionResult<Dtos.Teacher>>> Post(Dtos.Teacher? teacher)
        {
            if (teacher == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing teacher details"
                });
            }

            this.logger.LogInformation($"Adding new teacher '{teacher.Name}'");
            var command = new Commands.AddUser
            {
                Name = teacher.Name,
                Password = teacher.Password,
                Role = Data.UserRole.Teacher
            };
            return await this.executionEngine.ExecuteForHttp<Data.User, Dtos.Teacher>(
                command, s => Dtos.Teacher.FromModel(s!));
        }

        /// <summary>
        /// Updates an existing teacher.
        /// </summary>
        /// <param name="id">The name of the teacher.</param>
        /// <param name="user">The updated details.</param>
        /// <returns>The result of execution.</returns>

        [HttpPut("{id}")]
        public async Task<ActionResult<ExecutionResult<Dtos.Teacher>>> Put(string? id, Dtos.Teacher? user)
        {
            if ((user == null) || string.IsNullOrEmpty(id))
            {
                return this.BadRequest(new
                {
                    Error = "Missing user details"
                });
            }

            this.logger.LogInformation($"Updating teacher '{id}'");
            var command = new Commands.UpdateUser
            {
                CurrentName = id,
                Name = user.Name,
                Role = Data.UserRole.Teacher
            };
            return await this.executionEngine.ExecuteForHttp<Data.User, Dtos.Teacher>(
                command, s => Dtos.Teacher.FromModel(s!));
        }
    }
}
