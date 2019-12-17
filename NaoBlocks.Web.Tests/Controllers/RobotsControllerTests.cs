using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NaoBlocks.Core.Commands;
using NaoBlocks.Core.Models;
using NaoBlocks.Web.Controllers;
using Raven.Client.Documents.Session;
using RavenDB.Mocks;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class RobotsControllerTests
    {
        [Fact]
        public async Task DeleteRobotCallsCorrectCommand()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RobotsController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new RobotsController(loggerMock.Object, manager, sessionMock.Object);

            // Act
            await controller.Delete("Bob");

            // Assert
            var command = Assert.IsType<DeleteRobotCommand>(manager.LastCommand);
            Assert.Equal("Bob", command.MachineName);
        }

        [Fact]
        public async Task DeleteRobotChecksForInput()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RobotsController>>();
            var manager = new FakeCommandManager();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new RobotsController(loggerMock.Object, manager, sessionMock.Object);

            // Act
            var response = await controller.Delete(null);

            // Assert
            var actual = Assert.IsType<ActionResult<Dtos.ExecutionResult>>(response);
            Assert.IsType<BadRequestObjectResult>(actual.Result);
        }

        [Fact]
        public async Task DeleteRobotFailsExecution()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RobotsController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing()
                .SetupApplyError("Something failed");
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new RobotsController(loggerMock.Object, manager, sessionMock.Object);

            // Act
            var response = await controller.Delete("Bob");

            // Assert
            var actual = Assert.IsType<ActionResult<Dtos.ExecutionResult>>(response);
            var objectResult = Assert.IsType<ObjectResult>(actual.Result);
            Assert.Equal(500, objectResult.StatusCode);
            var innerResponse = Assert.IsType<Dtos.ExecutionResult>(objectResult.Value);
            Assert.Null(innerResponse.ValidationErrors);
            Assert.NotEmpty(innerResponse.ExecutionErrors);
        }

        [Fact]
        public async Task DeleteRobotFailsValidation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RobotsController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing()
                .SetupValidateErrors("Oops");
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new RobotsController(loggerMock.Object, manager, sessionMock.Object);

            // Act
            var response = await controller.Delete("Bob");

            // Assert
            var actual = Assert.IsType<ActionResult<Dtos.ExecutionResult>>(response);
            var badRequest = Assert.IsType<BadRequestObjectResult>(actual.Result);
            var innerResponse = Assert.IsType<Dtos.ExecutionResult>(badRequest.Value);
            Assert.NotEmpty(innerResponse.ValidationErrors);
            Assert.Null(innerResponse.ExecutionErrors);
        }

        [Fact]
        public async Task DeleteRobotRemovesRobot()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RobotsController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new RobotsController(loggerMock.Object, manager, sessionMock.Object);

            // Act
            var response = await controller.Delete("Bob");

            // Assert
            Assert.Null(response.Value.ValidationErrors);
            Assert.Null(response.Value.ExecutionErrors);
            Assert.Equal(1, manager.CountOfApplyCalled);
            Assert.Equal(1, manager.CountOfValidateCalled);
        }

        [Fact]
        public async Task GetRobotReturns404()
        {
            // Arrange
            var data = new Robot[0];
            var queryable = data.AsRavenQueryable();
            var loggerMock = new Mock<ILogger<RobotsController>>();
            var manager = new FakeCommandManager();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<Robot>(null, null, false)).Returns(() => queryable);
            var controller = new RobotsController(loggerMock.Object, manager, sessionMock.Object);

            // Act
            var response = await controller.GetRobot("BobTheBot");

            // Assert
            var actual = Assert.IsType<ActionResult<Dtos.Robot>>(response);
            var objectResult = Assert.IsType<NotFoundResult>(actual.Result);
            Assert.Null(actual.Value);
        }

        [Fact]
        public async Task GetRobotReturnsRobot()
        {
            // Arrange
            var data = new[]
            {
                new Robot { FriendlyName = "Bob", MachineName = "BobTheBot" }
            };
            var queryable = data.AsRavenQueryable();
            var loggerMock = new Mock<ILogger<RobotsController>>();
            var manager = new FakeCommandManager();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<Robot>(null, null, false)).Returns(() => queryable);
            var controller = new RobotsController(loggerMock.Object, manager, sessionMock.Object);

            // Act
            var response = await controller.GetRobot("BobTheBot");

            // Assert
            var actual = Assert.IsType<ActionResult<Dtos.Robot>>(response);
            Assert.Equal(data[0].MachineName, actual.Value.MachineName);
            Assert.Equal(data[0].FriendlyName, actual.Value.FriendlyName);
        }

        [Fact]
        public async Task PostAddsRobot()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RobotsController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new RobotsController(loggerMock.Object, manager, sessionMock.Object);
            var request = new Dtos.Robot { MachineName = "Bob" };

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
            var loggerMock = new Mock<ILogger<RobotsController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new RobotsController(loggerMock.Object, manager, sessionMock.Object);
            var request = new Dtos.Robot { MachineName = "r2d2", FriendlyName = "Bob" };

            // Act
            await controller.Post(request);

            // Assert
            var command = Assert.IsType<AddRobotCommand>(manager.LastCommand);
            Assert.Equal("Bob", command.FriendlyName);
            Assert.Equal("r2d2", command.MachineName);
        }

        [Fact]
        public async Task PostChecksForInput()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RobotsController>>();
            var manager = new FakeCommandManager();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new RobotsController(loggerMock.Object, manager, sessionMock.Object);

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
            var loggerMock = new Mock<ILogger<RobotsController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing()
                .SetupApplyError("Something failed");
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new RobotsController(loggerMock.Object, manager, sessionMock.Object);
            var request = new Dtos.Robot { MachineName = "Bob" };

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
            var loggerMock = new Mock<ILogger<RobotsController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing()
                .SetupValidateErrors("Oops");
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new RobotsController(loggerMock.Object, manager, sessionMock.Object);
            var request = new Dtos.Robot();

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