using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NaoBlocks.Core.Commands;
using NaoBlocks.Core.Models;
using NaoBlocks.Web.Controllers;
using Raven.Client.Documents.Session;
using RavenDB.Mocks;
using System;
using System.Threading.Tasks;
using Xunit;

using Data = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class SystemControllerTests
    {
        [Fact]
        public async Task InitialiseAddsAdministrator()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SystemController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing();
            var users = new User[0];
            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<User>(null, null, false)).Returns(users.AsRavenQueryable());
            var controller = new SystemController(loggerMock.Object, manager, sessionMock.Object);
            var request = new Data.User { Name = "Bob" };

            // Act
            var response = await controller.Initialise(request);

            // Assert
            Assert.Null(response.Value.ValidationErrors);
            Assert.Null(response.Value.ExecutionErrors);
            Assert.Equal(1, manager.CountOfApplyCalled);
            Assert.Equal(1, manager.CountOfValidateCalled);
        }

        [Fact]
        public async Task InitialiseCallsCorrectCommand()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SystemController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing();
            var users = new User[0];
            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<User>(null, null, false)).Returns(users.AsRavenQueryable());
            var controller = new SystemController(loggerMock.Object, manager, sessionMock.Object);
            var request = new Data.User { Name = "Bob", Password = "password" };

            // Act
            await controller.Initialise(request);

            // Assert
            var command = Assert.IsType<AddUserCommand>(manager.LastCommand);
            Assert.Equal("Bob", command.Name);
            Assert.Equal("password", command.Password);
            Assert.Equal(UserRole.Administrator, command.Role);
        }

        [Fact]
        public async Task InitialiseChecksForInput()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SystemController>>();
            var manager = new FakeCommandManager();
            var users = new User[0];
            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<User>(null, null, false)).Returns(users.AsRavenQueryable());
            var controller = new SystemController(loggerMock.Object, manager, sessionMock.Object);

            // Act
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await controller.Initialise(null));
        }

        [Fact]
        public async Task InitialiseFailsExecution()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SystemController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing()
                .SetupApplyError("Something failed");
            var users = new User[0];
            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<User>(null, null, false)).Returns(users.AsRavenQueryable());
            var controller = new SystemController(loggerMock.Object, manager, sessionMock.Object);
            var request = new Data.User { Name = "Bob" };

            // Act
            var response = await controller.Initialise(request);

            // Assert
            var actual = Assert.IsType<ActionResult<Data.ExecutionResult>>(response);
            var objectResult = Assert.IsType<ObjectResult>(actual.Result);
            Assert.Equal(500, objectResult.StatusCode);
            var innerResponse = Assert.IsType<Data.ExecutionResult>(objectResult.Value);
            Assert.Null(innerResponse.ValidationErrors);
            Assert.NotEmpty(innerResponse.ExecutionErrors);
        }

        [Fact]
        public async Task InitialiseFailsValidation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SystemController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing()
                .SetupValidateErrors("Oops");
            var users = new User[0];
            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<User>(null, null, false)).Returns(users.AsRavenQueryable());
            var controller = new SystemController(loggerMock.Object, manager, sessionMock.Object);
            var request = new Data.User();

            // Act
            var response = await controller.Initialise(request);

            // Assert
            var actual = Assert.IsType<ActionResult<Data.ExecutionResult>>(response);
            var badRequest = Assert.IsType<BadRequestObjectResult>(actual.Result);
            var innerResponse = Assert.IsType<Data.ExecutionResult>(badRequest.Value);
            Assert.NotEmpty(innerResponse.ValidationErrors);
            Assert.Null(innerResponse.ExecutionErrors);
        }

        [Fact]
        public async Task InitialiseIfAUserExists()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SystemController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing()
                .SetupValidateErrors("Oops");
            var users = new[] { new User() };
            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<User>(null, null, false)).Returns(users.AsRavenQueryable());
            var controller = new SystemController(loggerMock.Object, manager, sessionMock.Object);
            var request = new Data.User();

            // Act
            var response = await controller.Initialise(request);

            // Assert
            var actual = Assert.IsType<ActionResult<Data.ExecutionResult>>(response);
            var badRequest = Assert.IsType<BadRequestObjectResult>(actual.Result);
            var innerResponse = Assert.IsType<Data.ExecutionResult>(badRequest.Value);
            Assert.NotEmpty(innerResponse.ValidationErrors);
            Assert.Null(innerResponse.ExecutionErrors);
        }

        [Fact]
        public void VersionReturnsVersion()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SystemController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new SystemController(loggerMock.Object, manager, sessionMock.Object);

            // Act
            var result = controller.Version();

            // Assert
            Assert.IsType<ActionResult<object>>(result);
        }
    }
}