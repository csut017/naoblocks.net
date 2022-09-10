using Microsoft.AspNetCore.Mvc;
using Moq;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Controllers;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

using Data = NaoBlocks.Engine.Data;
using Generators = NaoBlocks.Engine.Generators;
using Transfer = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class StudentsControllerTests
    {
        [Fact]
        public async Task ClearLogsCallsCommand()
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<ClearProgramLogs>();
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.ClearLogs("Maia");

            // Assert
            var result = Assert.IsType<ExecutionResult>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();

            var command = Assert.IsType<ClearProgramLogs>(engine.LastCommand);
            Assert.Equal("Maia", command.UserName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ClearLogsValidatesName(string? name)
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.ClearLogs(name);

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public async Task ClearSnapshotsCallsCommand()
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<ClearSnapshots>();
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.ClearSnapshots("Maia");

            // Assert
            var result = Assert.IsType<ExecutionResult>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();

            var command = Assert.IsType<ClearSnapshots>(engine.LastCommand);
            Assert.Equal("Maia", command.UserName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ClearSnapshotsValidatesName(string? name)
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.ClearSnapshots(name);

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public async Task DeleteCallsDelete()
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<DeleteUser>();
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.Delete("Mia");

            // Assert
            var result = Assert.IsType<ExecutionResult>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();
        }

        [Theory]
        [InlineData(null, ReportFormat.Csv, "text/csv", "Students-List.csv")]
        [InlineData("xlsx", ReportFormat.Excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Students-List.xlsx")]
        [InlineData("Pdf", ReportFormat.Pdf, "application/pdf", "Students-List.pdf")]
        [InlineData("pdf", ReportFormat.Pdf, "application/pdf", "Students-List.pdf")]
        [InlineData("PDF", ReportFormat.Pdf, "application/pdf", "Students-List.pdf")]
        public async Task ExportDetailsGeneratesReport(string? format, ReportFormat expected, string contentType, string fileName)
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            var generator = new Mock<Generators.StudentExport>();
            var result = Tuple.Create((Stream)new MemoryStream(), fileName);
            generator.Setup(g => g.GenerateAsync(expected))
                .Returns(Task.FromResult(result))
                .Verifiable();
            generator.Setup(g => g.IsFormatAvailable(expected))
                .Returns(true)
                .Verifiable();
            engine.RegisterGenerator(generator.Object);
            GenerateUserDataQuery(engine, new Data.User
            {
                Name = "Mia",
                Role = Data.UserRole.Student
            });
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportDetails("Mia", format);

            // Assert
            var streamResult = Assert.IsType<FileStreamResult>(response);
            generator.Verify();
            Assert.Equal(contentType, streamResult.ContentType);
            Assert.Equal(fileName, streamResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportDetailsHandlesInvalidInput()
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            GenerateUserDataQuery(engine, new Data.User
            {
                Name = "Mia",
                Role = Data.UserRole.Student
            });
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportDetails("Mia", "garbage");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ExportDetailsHandlesInvalidStudentName(string? studentName)
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportDetails(studentName, "Pdf");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task ExportDetailsHandlesMissingStudent()
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            var generator = new Mock<Generators.StudentExport>();
            engine.RegisterGenerator(generator.Object);
            GenerateUserDataQuery(engine, null);
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportDetails("Mia", "Pdf");

            // Assert
            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task ExportDetailsHandlesUnknownFormat()
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            var generator = new Mock<Generators.StudentExport>();
            engine.RegisterGenerator(generator.Object);
            GenerateUserDataQuery(engine, new Data.User
            {
                Name = "Mia",
                Role = Data.UserRole.Student
            });
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportDetails("Mia", "Unknown");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Theory]
        [InlineData(null, ReportFormat.Excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Students-List.xlsx")]
        [InlineData("xlsx", ReportFormat.Excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Students-List.xlsx")]
        [InlineData("Pdf", ReportFormat.Pdf, "application/pdf", "Students-List.pdf")]
        [InlineData("pdf", ReportFormat.Pdf, "application/pdf", "Students-List.pdf")]
        [InlineData("PDF", ReportFormat.Pdf, "application/pdf", "Students-List.pdf")]
        public async Task ExportListGeneratesReport(string? format, ReportFormat expected, string contentType, string fileName)
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            var generator = new Mock<Generators.StudentsList>();
            var result = Tuple.Create((Stream)new MemoryStream(), fileName);
            generator.Setup(g => g.GenerateAsync(expected))
                .Returns(Task.FromResult(result))
                .Verifiable();
            generator.Setup(g => g.IsFormatAvailable(expected))
                .Returns(true)
                .Verifiable();
            engine.RegisterGenerator(generator.Object);
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportList(format);

            // Assert
            var streamResult = Assert.IsType<FileStreamResult>(response);
            generator.Verify();
            Assert.Equal(contentType, streamResult.ContentType);
            Assert.Equal(fileName, streamResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportListHandlesInvalidInput()
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportList("garbage");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task ExportListHandlesUnknownFormat()
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            var generator = new Mock<Generators.StudentsList>();
            engine.RegisterGenerator(generator.Object);
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportList("unknown");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Theory]
        [InlineData(null, ReportFormat.Csv, "text/csv", "Students-List.csv")]
        [InlineData("xlsx", ReportFormat.Excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Students-List.xlsx")]
        [InlineData("Pdf", ReportFormat.Pdf, "application/pdf", "Students-List.pdf")]
        [InlineData("pdf", ReportFormat.Pdf, "application/pdf", "Students-List.pdf")]
        [InlineData("PDF", ReportFormat.Pdf, "application/pdf", "Students-List.pdf")]
        public async Task ExportLogsGeneratesReport(string? format, ReportFormat expected, string contentType, string fileName)
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            var generator = new Mock<Generators.ProgramLogsList>();
            var result = Tuple.Create((Stream)new MemoryStream(), fileName);
            generator.Setup(g => g.GenerateAsync(expected))
                .Returns(Task.FromResult(result))
                .Verifiable();
            generator.Setup(g => g.IsFormatAvailable(expected))
                .Returns(true)
                .Verifiable();
            engine.RegisterGenerator(generator.Object);
            GenerateUserDataQuery(engine, new Data.User
            {
                Name = "Mia",
                Role = Data.UserRole.Student
            });
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportLogs("Mia", format);

            // Assert
            var streamResult = Assert.IsType<FileStreamResult>(response);
            generator.Verify();
            Assert.Equal(contentType, streamResult.ContentType);
            Assert.Equal(fileName, streamResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportLogsHandlesInvalidInput()
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            GenerateUserDataQuery(engine, new Data.User
            {
                Name = "Mia",
                Role = Data.UserRole.Student
            });
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportLogs("Mia", "garbage");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ExportLogsHandlesInvalidStudentName(string? studentName)
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportLogs(studentName, "Pdf");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task ExportLogsHandlesMissingStudent()
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            var generator = new Mock<Generators.ProgramLogsList>();
            engine.RegisterGenerator(generator.Object);
            GenerateUserDataQuery(engine, null);
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportLogs("Mia", "Pdf");

            // Assert
            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task ExportLogsHandlesUnknownFormat()
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            var generator = new Mock<Generators.ProgramLogsList>();
            engine.RegisterGenerator(generator.Object);
            GenerateUserDataQuery(engine, new Data.User
            {
                Name = "Mia",
                Role = Data.UserRole.Student
            });
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportLogs("Mia", "Unknown");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Theory]
        [InlineData(null, ReportFormat.Csv, "text/csv", "Students-List.csv")]
        [InlineData("xlsx", ReportFormat.Excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Students-List.xlsx")]
        [InlineData("Pdf", ReportFormat.Pdf, "application/pdf", "Students-List.pdf")]
        [InlineData("pdf", ReportFormat.Pdf, "application/pdf", "Students-List.pdf")]
        [InlineData("PDF", ReportFormat.Pdf, "application/pdf", "Students-List.pdf")]
        public async Task ExportSnapshotsGeneratesReport(string? format, ReportFormat expected, string contentType, string fileName)
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            var generator = new Mock<Generators.SnapshotsList>();
            var result = Tuple.Create((Stream)new MemoryStream(), fileName);
            generator.Setup(g => g.GenerateAsync(expected))
                .Returns(Task.FromResult(result))
                .Verifiable();
            generator.Setup(g => g.IsFormatAvailable(expected))
                .Returns(true)
                .Verifiable();
            engine.RegisterGenerator(generator.Object);
            GenerateUserDataQuery(engine, new Data.User
            {
                Name = "Mia",
                Role = Data.UserRole.Student
            });
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportSnapshots("Mia", format);

            // Assert
            var streamResult = Assert.IsType<FileStreamResult>(response);
            generator.Verify();
            Assert.Equal(contentType, streamResult.ContentType);
            Assert.Equal(fileName, streamResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportSnapshotsHandlesInvalidInput()
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            GenerateUserDataQuery(engine, new Data.User
            {
                Name = "Mia",
                Role = Data.UserRole.Student
            });
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportSnapshots("Mia", "garbage");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ExportSnapshotsHandlesInvalidStudentName(string? studentName)
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportSnapshots(studentName, "Pdf");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task ExportSnapshotsHandlesMissingStudent()
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            var generator = new Mock<Generators.SnapshotsList>();
            engine.RegisterGenerator(generator.Object);
            GenerateUserDataQuery(engine, null);
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportSnapshots("Mia", "Pdf");

            // Assert
            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task ExportSnapshotsHandlesUnknownFormat()
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            var generator = new Mock<Generators.SnapshotsList>();
            engine.RegisterGenerator(generator.Object);
            GenerateUserDataQuery(engine, new Data.User
            {
                Name = "Mia",
                Role = Data.UserRole.Student
            });
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportSnapshots("Mia", "Unknown");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task GetStudentRetrievesStudentHandlesMissingStudent()
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            var query = new Mock<UserData>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("Mia"))
                .Returns(Task.FromResult((Data.User?)null));
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.GetStudent("Mia");

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task GetStudentRetrievesStudentHandlesNonStudentRole()
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            var query = new Mock<UserData>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("Mia"))
                .Returns(Task.FromResult((Data.User?)new Data.User
                {
                    Name = "Mia",
                    Role = Data.UserRole.Teacher
                }));
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.GetStudent("Mia");

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task GetStudentRetrievesStudentViaQuery()
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            var query = new Mock<UserData>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("Mia"))
                .Returns(Task.FromResult((Data.User?)new Data.User
                {
                    Name = "Mia",
                    Role = Data.UserRole.Student
                }));
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.GetStudent("Mia");

            // Assert
            Assert.NotNull(response.Value);
            Assert.Equal("Mia", response.Value?.Name);
        }

        [Fact]
        public async Task GetStudentsHandlesNullData()
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            var query = new Mock<UserData>();
            var result = new ListResult<Data.User>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrievePageAsync(0, 25, Data.UserRole.Student))
                .Returns(Task.FromResult(result));
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.GetStudents(null, null);

            // Assert
            Assert.Equal(0, response.Count);
            Assert.Equal(0, response.Page);
            Assert.Null(response.Items);
        }

        [Fact]
        public async Task GetStudentsRetrievesViaQuery()
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            var query = new Mock<UserData>();
            var result = ListResult.New(
                new[] {
                    new Data.User { Role = Data.UserRole.Student }
                });
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrievePageAsync(0, 25, Data.UserRole.Student))
                .Returns(Task.FromResult(result));
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.GetStudents(null, null);

            // Assert
            Assert.Equal(1, response.Count);
            Assert.Equal(0, response.Page);
            Assert.NotEmpty(response.Items);
        }

        [Fact]
        public async Task PostCallsAddUser()
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<AddUser>(
                CommandResult.New(1, new Data.User()));
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var Student = new Transfer.Student { Name = "Mia" };
            var response = await controller.Post(Student);

            // Assert
            var result = Assert.IsType<ExecutionResult<Transfer.Student>>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();
            var command = Assert.IsType<AddUser>(engine.LastCommand);
            Assert.Equal(Data.UserRole.Student, command.Role);
        }

        [Fact]
        public async Task PostValidatesIncomingData()
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<AddUser>();
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.Post(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public async Task PutCallsUpdateStudent()
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<UpdateUser>(
                CommandResult.New(1, new Data.User()));
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var Student = new Transfer.Student { Name = "Mia", Role = "Student" };
            var response = await controller.Put("Maia", Student);

            // Assert
            var result = Assert.IsType<ExecutionResult<Transfer.Student>>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();

            var command = Assert.IsType<UpdateUser>(engine.LastCommand);
            Assert.Equal(Data.UserRole.Student, command.Role);
        }

        [Fact]
        public async Task PutValidatesIncomingData()
        {
            // Arrange
            var logger = new FakeLogger<StudentsController>();
            var engine = new FakeEngine();
            var controller = new StudentsController(
                logger,
                engine);

            // Act
            var response = await controller.Put("Maia", null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        private static void GenerateUserDataQuery(FakeEngine engine, Data.User? user)
        {
            var query = new Mock<UserData>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("Mia"))
                .Returns(Task.FromResult((Data.User?)user));
        }
    }
}