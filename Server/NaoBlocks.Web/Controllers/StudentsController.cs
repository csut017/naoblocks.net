using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NaoBlocks.Common;
using NaoBlocks.Communications;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Configuration;
using NaoBlocks.Web.Helpers;
using QRCoder;
using System.Text;
using System.Web;

using Data = NaoBlocks.Engine.Data;
using Generators = NaoBlocks.Engine.Generators;
using Transfer = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Controllers
{
    /// <summary>
    /// A controller for working with students.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Policy = "Teacher")]
    [Produces("application/json")]
    public class StudentsController : ControllerBase
    {
        private readonly IOptions<Addresses> configuration;
        private readonly IExecutionEngine executionEngine;
        private readonly ILogger<StudentsController> logger;

        /// <summary>
        /// Initialises a new <see cref="UsersController"/> instance.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="engine">The execution engine for processing commands and queries.</param>
        /// <param name="configuration">The address configuration.</param>
        public StudentsController(ILogger<StudentsController> logger, IExecutionEngine engine, IOptions<Addresses> configuration)
        {
            this.logger = logger;
            this.executionEngine = engine;
            this.configuration = configuration;
        }

        /// <summary>
        /// Deletes all the program logs for a student.
        /// </summary>
        /// <param name="name">The name of the student.</param>
        /// <returns>The result of execution.</returns>
        [HttpDelete("{name}/logs")]
        public async Task<ActionResult<ExecutionResult>> ClearLogs(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return this.BadRequest(new
                {
                    Error = "Missing student name"
                });
            }

            this.logger.LogInformation("Clearing logs for student '{name}'", name);
            var command = new ClearProgramLogs
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
        [HttpDelete("{name}/snapshots")]
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

            this.logger.LogInformation("Clearing snapshots for student '{name}'", name);
            var command = new ClearSnapshots
            {
                UserName = name
            };
            return await this.executionEngine.ExecuteForHttp(command);
        }

        /// <summary>
        /// Deletes a student by their name.
        /// </summary>
        /// <param name="name">The name of the student.</param>
        /// <returns>The result of execution.</returns>
        [HttpDelete("{name}")]
        public async Task<ActionResult<ExecutionResult>> Delete(string name)
        {
            this.logger.LogInformation("Deleting student '{name}'", name);
            var command = new DeleteUser
            {
                Name = name,
                Role = Data.UserRole.Student
            };
            return await this.executionEngine.ExecuteForHttp(command);
        }

        /// <summary>
        /// Exports the details on a student.
        /// </summary>
        /// <param name="name">The name of the student.</param>
        /// <param name="format">The report format to generate.</param>
        /// <returns>The generated student details.</returns>
        /// <param name="from">The start date.</param>
        /// <param name="to">The end date.</param>
        [HttpGet("{name}/export")]
        [HttpGet("{name}/export{format}")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult> ExportDetails(string? name, string? format = ".xlsx", string? from = null, string? to = null)
        {
            this.logger.LogInformation("Generating student details export");
            var args = this.MakeArgs($"from={from}", $"to={to}");
            return await this.GenerateUserReport<Generators.StudentExport>(
                this.executionEngine,
                format,
                name,
                defaultFormat: ReportFormat.Excel,
                args: args);
        }

        /// <summary>
        /// Generates the student list export.
        /// </summary>
        /// <param name="format">The format to use.</param>
        /// <returns>The generated student list.</returns>
        [HttpGet("export")]
        [HttpGet("export{format}")]
        public async Task<ActionResult> ExportList(string? format)
        {
            this.logger.LogInformation("Generating student list export");
            return await this.GenerateReport<Generators.StudentsList>(
                this.executionEngine,
                format);
        }

        /// <summary>
        /// Exports the logs for a student.
        /// </summary>
        /// <param name="name">The name of the student.</param>
        /// <param name="format">The report format to generate.</param>
        /// <returns>The generated student logs.</returns>
        /// <param name="from">The start date.</param>
        /// <param name="to">The end date.</param>
        [HttpGet("{name}/logs/export")]
        [HttpGet("{name}/logs/export{format}")]
        public async Task<ActionResult> ExportLogs(string? name, string? format = ".xlsx", string? from = null, string? to = null)
        {
            this.logger.LogInformation("Generating student log export");
            var args = this.MakeArgs($"from={from}", $"to={to}");
            return await this.GenerateUserReport<Generators.UserLogs>(
                this.executionEngine,
                format,
                name,
                defaultFormat: ReportFormat.Excel,
                args: args);
        }

        /// <summary>
        /// Exports the snapshots for a student.
        /// </summary>
        /// <param name="name">The name of the student.</param>
        /// <param name="format">The report format to generate.</param>
        /// <returns>The generated snapshots list.</returns>
        /// <param name="from">The start date.</param>
        /// <param name="to">The end date.</param>
        [HttpGet("{name}/snapshots/export")]
        [HttpGet("{name}/snapshots/export{format}")]
        public async Task<ActionResult> ExportSnapshots(string? name, string? format = ".xlsx", string? from = null, string? to = null)
        {
            this.logger.LogInformation("Generating student snapshots export");
            var args = this.MakeArgs($"from={from}", $"to={to}");
            return await this.GenerateUserReport<Generators.UserSnapshots>(
                this.executionEngine,
                format,
                name,
                defaultFormat: ReportFormat.Excel,
                args: args);
        }

        /// <summary>
        /// Retrieves a student by their name.
        /// </summary>
        /// <param name="name">The name of the student.</param>
        /// <returns>Either a 404 (not found) or the student details.</returns>
        [HttpGet("{name}")]
        public async Task<ActionResult<Dtos.Student>> GetStudent(string name)
        {
            this.logger.LogInformation("Retrieving student: {name}", name);
            var student = await this.executionEngine
                .Query<UserData>()
                .RetrieveByNameAsync(name)
                .ConfigureAwait(false);
            if ((student == null) || (student.Role != Data.UserRole.Student))
            {
                return NotFound();
            }

            this.logger.LogInformation("Retrieved student");
            return Dtos.Student.FromModel(student, Dtos.DetailsType.Standard);
        }

        /// <summary>
        /// Generates a QR code for a student.
        /// </summary>
        /// <param name="name">The name of the student.</param>
        /// <param name="format">The format to generate.</param>
        /// <param name="view">The default view to use.</param>
        /// <returns>Either a 404 (not found) or the user details.</returns>
        [HttpGet("{name}/qrcode")]
        [HttpGet("{name}/qrcode{format}")]
        public async Task<ActionResult> GetStudentQRCode(string name, string? format = ".png", [FromQuery] string? view = null)
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

            var student = result.Value.Output;
            this.logger.LogInformation("Login token generated for {name}", name);
            var address = ClientAddressList.RetrieveAddresses().First();
            var key = Convert.ToBase64String(Encoding.UTF8.GetBytes($"token:{student?.PlainPassword},view:{view}"));
            var fullAddress = $"https://{address}:{this.configuration.Value.HttpsPort}/login?key={HttpUtility.UrlEncode(key)}";
            this.logger.LogInformation("Loging URL is {url}", fullAddress);

            using var generator = new QRCodeGenerator();
            var codeData = generator.CreateQrCode(fullAddress, QRCodeGenerator.ECCLevel.Q);
            using var code = new PngByteQRCode(codeData);
            var image = code.GetGraphic(20);
            return File(image, ContentTypes.Png, $"{name}{format}");
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

            this.logger.LogInformation("Retrieving students: page {pageNum} with size {pageSize}", pageNum, pageSize);
            var dataPage = await this.executionEngine
                .Query<UserData>()
                .RetrievePageAsync(pageNum, pageSize, Data.UserRole.Student)
                .ConfigureAwait(false);
            var count = dataPage.Items?.Count() ?? 0;
            this.logger.LogInformation("Retrieved {count} students", count);
            var result = new ListResult<Dtos.Student>
            {
                Count = dataPage.Count,
                Page = pageNum,
                Items = dataPage.Items?.Select(s => Dtos.Student.FromModel(s))
            };
            return result;
        }

        /// <summary>
        /// Imports a list of students.
        /// </summary>
        /// <param name="action">The action to perform. Options are "parse".</param>
        /// <returns>The result of execution.</returns>
        [HttpPost("import")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult<ExecutionResult<ListResult<Transfer.User>>>> Import([FromQuery] string? action)
        {
            if (!"parse".Equals(action))
            {
                return this.BadRequest(new
                {
                    Error = $"Action {action} is invalid"
                });
            }

            var files = this.Request?.Form?.Files;
            if (files == null)
            {
                return this.BadRequest(new
                {
                    Error = "Expected a file to process"
                });
            }

            if (files.Count != 1)
            {
                return this.BadRequest(new
                {
                    Error = "Invalid number of files, can only handle one file at a time"
                });
            }

            this.logger.LogInformation("Parsing students import");
            using var inputStream = files[0].OpenReadStream();
            var command = new ParseUsersImport
            {
                Data = inputStream,
                Role = Data.UserRole.Student
            };

            return await this.executionEngine
                .ExecuteForHttp<IEnumerable<Data.ItemImport<Data.User>>, ListResult<Transfer.User>>
                (command,
                users => ListResult.New(users.Select(r => Transfer.User.FromModel(r!, Transfer.DetailsType.Standard | Transfer.DetailsType.Parse))));
        }

        /// <summary>
        /// Adds a new student.
        /// </summary>
        /// <param name="student">The details of the student.</param>
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

            this.logger.LogInformation("Adding new student '{name}'", student.Name);
            var command = new AddUser
            {
                Name = student.Name,
                Password = student.Password,
                Role = Data.UserRole.Student,
                Settings = student.Settings,
                Age = student.Age,
                Gender = student.Gender
            };
            return await this.executionEngine.ExecuteForHttp<Data.User, Dtos.Student>(
                command, s => Dtos.Student.FromModel(s!, Dtos.DetailsType.Standard));
        }

        /// <summary>
        /// Updates an existing student.
        /// </summary>
        /// <param name="name">The name of the student.</param>
        /// <param name="student">The updated details.</param>
        /// <returns>The result of execution.</returns>
        [HttpPut("{name}")]
        public async Task<ActionResult<ExecutionResult<Dtos.Student>>> Put(string? name, Dtos.Student? student)
        {
            if ((student == null) || string.IsNullOrEmpty(name))
            {
                return this.BadRequest(new
                {
                    Error = "Missing student details"
                });
            }

            this.logger.LogInformation("Updating student '{name}'", name);
            var command = new UpdateUser
            {
                CurrentName = name,
                Name = student.Name,
                Password = student.Password,
                Role = Data.UserRole.Student,
                Settings = student.Settings,
                Age = student.Age,
                Gender = student.Gender
            };
            return await this.executionEngine.ExecuteForHttp<Data.User, Dtos.Student>(
                command, s => Dtos.Student.FromModel(s!, Dtos.DetailsType.Standard));
        }
    }
}