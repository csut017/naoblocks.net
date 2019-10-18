using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NaoBlocks.Web.Controllers;
using Raven.Client.Documents.Session;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class StudentsControllerTests
    {
        [Fact]
        public async Task CompileFailsExecution()
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
        public async Task CompileFailsValidation()
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
    }
}