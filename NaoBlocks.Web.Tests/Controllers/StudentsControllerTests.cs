using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NaoBlocks.Core.Commands;
using NaoBlocks.Core.Models;
using NaoBlocks.Web.Controllers;
using Raven.Client.Documents.Session;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class StudentsControllerTests
    {
        [Fact]
        public async Task DeleteStudentCallsCorrectCommand()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<StudentsController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new StudentsController(loggerMock.Object, manager, sessionMock.Object);

            // Act
            await controller.DeleteStudent("Bob");

            // Assert
            var command = Assert.IsType<DeleteUserCommand>(manager.LastCommand);
            Assert.Equal("Bob", command.Name);
            Assert.Equal(UserRole.Student, command.Role);
        }

        [Fact]
        public async Task DeleteStudentChecksForInput()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<StudentsController>>();
            var manager = new FakeCommandManager();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new StudentsController(loggerMock.Object, manager, sessionMock.Object);

            // Act
            var response = await controller.DeleteStudent(null);

            // Assert
            var actual = Assert.IsType<ActionResult<Dtos.ExecutionResult>>(response);
            Assert.IsType<BadRequestObjectResult>(actual.Result);
        }

        [Fact]
        public async Task DeleteStudentFailsExecution()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<StudentsController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing()
                .SetupApplyError("Something failed");
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new StudentsController(loggerMock.Object, manager, sessionMock.Object);

            // Act
            var response = await controller.DeleteStudent("Bob");

            // Assert
            var actual = Assert.IsType<ActionResult<Dtos.ExecutionResult>>(response);
            var objectResult = Assert.IsType<ObjectResult>(actual.Result);
            Assert.Equal(500, objectResult.StatusCode);
            var innerResponse = Assert.IsType<Dtos.ExecutionResult>(objectResult.Value);
            Assert.Null(innerResponse.ValidationErrors);
            Assert.NotEmpty(innerResponse.ExecutionErrors);
        }

        [Fact]
        public async Task DeleteStudentFailsValidation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<StudentsController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing()
                .SetupValidateErrors("Oops");
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new StudentsController(loggerMock.Object, manager, sessionMock.Object);

            // Act
            var response = await controller.DeleteStudent("Bob");

            // Assert
            var actual = Assert.IsType<ActionResult<Dtos.ExecutionResult>>(response);
            var badRequest = Assert.IsType<BadRequestObjectResult>(actual.Result);
            var innerResponse = Assert.IsType<Dtos.ExecutionResult>(badRequest.Value);
            Assert.NotEmpty(innerResponse.ValidationErrors);
            Assert.Null(innerResponse.ExecutionErrors);
        }

        [Fact]
        public async Task DeleteStudentRemovesStudent()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<StudentsController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new StudentsController(loggerMock.Object, manager, sessionMock.Object);

            // Act
            var response = await controller.DeleteStudent("Bob");

            // Assert
            Assert.Null(response.Value.ValidationErrors);
            Assert.Null(response.Value.ExecutionErrors);
            Assert.Equal(1, manager.CountOfApplyCalled);
            Assert.Equal(1, manager.CountOfValidateCalled);
        }

        [Fact]
        public async Task PostAddsStudent()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<StudentsController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new StudentsController(loggerMock.Object, manager, sessionMock.Object);
            var request = new Dtos.Student { Name = "Bob" };

            // Act
            var response = await controller.Post(request);

            // Assert
            Assert.Null(response.Value.ValidationErrors);
            Assert.Null(response.Value.ExecutionErrors);
            Assert.Equal(1, manager.CountOfApplyCalled);
            Assert.Equal(1, manager.CountOfValidateCalled);
        }

        [Fact]
        public async Task PostCallsCorrectCommand()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<StudentsController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new StudentsController(loggerMock.Object, manager, sessionMock.Object);
            var request = new Dtos.Student { Name = "Bob", Password = "password" };

            // Act
            await controller.Post(request);

            // Assert
            var command = Assert.IsType<AddUserCommand>(manager.LastCommand);
            Assert.Equal("Bob", command.Name);
            Assert.Equal("password", command.Password);
            Assert.Equal(UserRole.Student, command.Role);
        }

        [Fact]
        public async Task PostChecksForInput()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<StudentsController>>();
            var manager = new FakeCommandManager();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new StudentsController(loggerMock.Object, manager, sessionMock.Object);

            // Act
            var response = await controller.Post(null);

            // Assert
            var actual = Assert.IsType<ActionResult<Dtos.ExecutionResult>>(response);
            Assert.IsType<BadRequestObjectResult>(actual.Result);
        }

        [Fact]
        public async Task PostFailsExecution()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<StudentsController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing()
                .SetupApplyError("Something failed");
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new StudentsController(loggerMock.Object, manager, sessionMock.Object);
            var request = new Dtos.Student { Name = "Bob" };

            // Act
            var response = await controller.Post(request);

            // Assert
            var actual = Assert.IsType<ActionResult<Dtos.ExecutionResult>>(response);
            var objectResult = Assert.IsType<ObjectResult>(actual.Result);
            Assert.Equal(500, objectResult.StatusCode);
            var innerResponse = Assert.IsType<Dtos.ExecutionResult>(objectResult.Value);
            Assert.Null(innerResponse.ValidationErrors);
            Assert.NotEmpty(innerResponse.ExecutionErrors);
        }

        [Fact]
        public async Task PostFailsValidation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<StudentsController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing()
                .SetupValidateErrors("Oops");
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new StudentsController(loggerMock.Object, manager, sessionMock.Object);
            var request = new Dtos.Student();

            // Act
            var response = await controller.Post(request);

            // Assert
            var actual = Assert.IsType<ActionResult<Dtos.ExecutionResult>>(response);
            var badRequest = Assert.IsType<BadRequestObjectResult>(actual.Result);
            var innerResponse = Assert.IsType<Dtos.ExecutionResult>(badRequest.Value);
            Assert.NotEmpty(innerResponse.ValidationErrors);
            Assert.Null(innerResponse.ExecutionErrors);
        }
    }
}