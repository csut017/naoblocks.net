﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Helpers;
using Commands = NaoBlocks.Engine.Commands;
using Data = NaoBlocks.Engine.Data;
using Generators = NaoBlocks.Engine.Generators;

namespace NaoBlocks.Web.Controllers
{
    /// <summary>
    /// A controller for working with students.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Policy = "Teacher")]
    public class StudentsController : ControllerBase
    {
        private readonly ILogger<StudentsController> logger;
        private readonly IExecutionEngine executionEngine;

        /// <summary>
        /// Initialises a new <see cref="UsersController"/> instance.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="executionEngine">The execution engine for processing commands and queries.</param>
        public StudentsController(ILogger<StudentsController> logger, IExecutionEngine engine)
        {
            this.logger = logger;
            this.executionEngine = engine;
        }

        /// <summary>
        /// Deletes a student by their name.
        /// </summary>
        /// <param name="id">The name of the student.</param>
        /// <returns>The result of execution.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ExecutionResult>> Delete(string id)
        {
            this.logger.LogInformation($"Deleting student '{id}'");
            var command = new Commands.DeleteUser
            {
                Name = id,
                Role = Data.UserRole.Student
            };
            return await this.executionEngine.ExecuteForHttp(command);
        }

        /// <summary>
        /// Retrieves a student by their name.
        /// </summary>
        /// <param name="name">The name of the student.</param>
        /// <returns>Either a 404 (not found) or the student details.</returns>
        [HttpGet("{name}")]
        public async Task<ActionResult<Dtos.Student>> GetStudent(string name)
        {
            this.logger.LogDebug($"Retrieving student: id {name}");
            var student = await this.executionEngine
                .Query<UserData>()
                .RetrieveByNameAsync(name)
                .ConfigureAwait(false);
            if ((student == null) || (student.Role != Data.UserRole.Student))
            {
                return NotFound();
            }

            this.logger.LogDebug("Retrieved student");
            return Dtos.Student.FromModel(student, true);
        }

        /// <summary>
        /// Retrieves a page of students.
        /// </summary>
        /// <param name="page">The page number.</param>
        /// <param name="size">The number of records.</param>
        /// <returns>A <see cref="ListResult{TData}"/> containing the students.</returns>
        [HttpGet]
        public async Task<ListResult<Dtos.Student>> GetStudents(int? page, int? size)
        {
            (int pageNum, int pageSize) = this.ValidatePageArguments(page, size);

            this.logger.LogDebug($"Retrieving students: page {pageNum} with size {pageSize}");
            var dataPage = await this.executionEngine
                .Query<UserData>()
                .RetrievePageAsync(pageNum, pageSize, Data.UserRole.Student)
                .ConfigureAwait(false);
            var count = dataPage.Items?.Count() ?? 0;
            this.logger.LogDebug($"Retrieved {count} students");
            var result = new ListResult<Dtos.Student>
            {
                Count = dataPage.Count,
                Page = pageNum,
                Items = dataPage.Items?.Select(s => Dtos.Student.FromModel(s))
            };
            return result;
        }

        /// <summary>
        /// Adds a new student.
        /// </summary>
        /// <param name="teacher">The details of the student.</param>
        /// <returns>The result of execution.</returns>
        [HttpPost]
        public async Task<ActionResult<ExecutionResult<Dtos.Student>>> Post(Dtos.Student? student)
        {
            if (student == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing student details"
                });
            }

            this.logger.LogInformation($"Adding new student '{student.Name}'");
            var command = new Commands.AddUser
            {
                Name = student.Name,
                Password = student.Password,
                Role = Data.UserRole.Student,
                Settings = student.Settings,
                Age = student.Age,
                Gender = student.Gender
            };
            return await this.executionEngine.ExecuteForHttp<Data.User, Dtos.Student>(
                command, s => Dtos.Student.FromModel(s!, true));
        }

        /// <summary>
        /// Updates an existing student.
        /// </summary>
        /// <param name="id">The name of the student.</param>
        /// <param name="student">The updated details.</param>
        /// <returns>The result of execution.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ExecutionResult<Dtos.Student>>> Put(string? id, Dtos.Student? student)
        {
            if ((student == null) || string.IsNullOrEmpty(id))
            {
                return this.BadRequest(new
                {
                    Error = "Missing student details"
                });
            }

            this.logger.LogInformation($"Updating student '{id}'");
            var command = new Commands.UpdateUser
            {
                CurrentName = id,
                Name = student.Name,
                Password = student.Password,
                Role = Data.UserRole.Student,
                Settings = student.Settings,
                Age = student.Age,
                Gender = student.Gender
            };
            return await this.executionEngine.ExecuteForHttp<Data.User, Dtos.Student>(
                command, s => Dtos.Student.FromModel(s!, true));
        }

        /// <summary>
        /// Generates the student list export.
        /// </summary>
        /// <param name="format">The format to use.</param>
        /// <returns>The generated student list.</returns>
        [HttpGet("export")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult> ExportList(string? format)
        {
            return await this.GenerateReport<Generators.StudentsList>(
                this.executionEngine,
                format);
        }

        /// <summary>
        /// Exports the details on a student.
        /// </summary>
        /// <param name="name">The name of the student.</param>
        /// <returns>The generated student details.</returns>
        [HttpGet("{name}/export")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult> ExportDetails(string? name, string? format)
        {
            return await this.GenerateUserReport<Generators.StudentExport>(
                this.executionEngine,
                format,
                name);
        }

        /// <summary>
        /// Exports the logs for a student.
        /// </summary>
        /// <param name="name">The name of the student.</param>
        /// <returns>The generated student logs.</returns>
        [HttpGet("{id}/logs/export")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult> ExportLogs(string? name, string? format)
        {
            return await this.GenerateUserReport<Generators.ProgramLogsList>(
                this.executionEngine,
                format,
                name);
        }

        /// <summary>
        /// Exports the snapshots for a student.
        /// </summary>
        /// <param name="name">The name of the student.</param>
        /// <returns>The generated snapshots list.</returns>
        [HttpGet("{id}/snapshots/export")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult> ExportSnapshots(string? name, string? format)
        {
            return await this.GenerateUserReport<Generators.SnapshotsLists>(
                this.executionEngine,
                format,
                name);
        }

        /*
        /// <summary>
        /// Generates a student's QR code.
        /// </summary>
        /// <param name="name">The name of the student.</param>
        /// <returns>Either a 404 (not found) or the user details.</returns>
        [HttpGet("{id}/qrcode")]
        [AllowAnonymous]
        public async Task<ActionResult> GetStudentQRCode(string id, string? force)
        {
            this._logger.LogDebug($"Generating login token for {id}");
            var command = new Commands.GenerateLoginToken
            {
                Name = id,
                OverrideExisting = string.Equals(force, "yes", StringComparison.OrdinalIgnoreCase)
            };
            var result = await this.executionEngine.ExecuteForHttp(command, i => i);
            if (result?.Value?.Successful != true)
            {
                return result.Result;
            }

            var student = result.Value.Output;
            this._logger.LogDebug("Login token generated");

            var config = await this.session.Query<SystemValues>()
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
            var address = config?.DefaultAddress ?? string.Empty;
            this._logger.LogDebug("Retrieved system address");

            var fullAddress = address + "/login?key=" + HttpUtility.UrlEncode(student.LoginToken);
            this._logger.LogInformation($"Generating QR code for {fullAddress}");
            using var generator = new QRCodeGenerator();
            var codeData = generator.CreateQrCode(fullAddress, QRCodeGenerator.ECCLevel.Q);
            using var code = new QRCode(codeData);
            var image = code.GetGraphic(20);
            using var stream = new MemoryStream();
            image.Save(stream, ImageFormat.Png);
            return File(stream.ToArray(), ContentTypes.Png);
        }
        */

        /// <summary>
        /// Deletes all the program logs for a student.
        /// </summary>
        /// <param name="name">The name of the student.</param>
        /// <returns>The result of execution.</returns>
        [HttpDelete("{id}/logs")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult<ExecutionResult>> ClearLogs(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return this.BadRequest(new
                {
                    Error = "Missing student name"
                });
            }

            this.logger.LogInformation($"Clearing logs for student '{name}'");
            var command = new Commands.ClearProgramLogs
            {
                UserName = name
            };
            return await this.executionEngine.ExecuteForHttp(command);
        }

        /// <summary>
        /// Deletes all the snapshots for a student.
        /// </summary>
        /// <param name="name">The name of the student.</param>
        /// <returns>The result of execution.</returns>
        [HttpDelete("{id}/snapshots")]
        [Authorize(Policy = "Administrator")]
        public async Task<ActionResult<ExecutionResult>> ClearSnapshots(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return this.BadRequest(new
                {
                    Error = "Missing student name"
                });
            }

            this.logger.LogInformation($"Clearing snapshots for student '{name}'");
            var command = new Commands.ClearSnapshots
            {
                UserName = name
            };
            return await this.executionEngine.ExecuteForHttp(command);
        }
    }
}