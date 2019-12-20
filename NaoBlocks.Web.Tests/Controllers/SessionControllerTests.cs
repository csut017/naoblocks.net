using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NaoBlocks.Core.Commands;
using NaoBlocks.Core.Models;
using NaoBlocks.Web.Controllers;
using NaoBlocks.Web.Helpers;
using Raven.Client.Documents.Session;
using System;
using System.Threading.Tasks;
using Xunit;

using Data = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class SessionControllerTests
    {
        [Fact]
        public async Task GetFailsIfUserNotSet()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing()
                .SetupValidateErrors("Oops");
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new SessionController(loggerMock.Object, manager, sessionMock.Object, InitialiseOptions().Object);

            // Act
            var response = await controller.Get();

            // Assert
            var actual = Assert.IsType<ActionResult<Data.User>>(response);
            Assert.IsType<NotFoundResult>(actual.Result);
        }

        [Fact]
        public async Task GetReturnsCurrentUser()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing()
                .SetupValidateErrors("Oops");
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new SessionController(loggerMock.Object, manager, sessionMock.Object, InitialiseOptions().Object);
            Utils.InitialiseUser(sessionMock, controller, new User { Id = "users/1", Name = "Bob" });

            // Act
            var response = await controller.Get();

            // Assert
            var actual = Assert.IsType<ActionResult<Data.User>>(response);
            var user = Assert.IsType<Data.User>(actual.Value);
            Assert.Equal("Bob", user.Name);
        }

        [Fact]
        public async Task PostCallsCorrectCommand()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing()
                .SetupApply(cmd =>
                {
                    ((StartSessionCommand)cmd).Output = new Session
                    {
                        WhenExpires = DateTime.UtcNow.AddDays(1)
                    };
                    return Task.FromResult(new CommandResult(1));
                });
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new SessionController(loggerMock.Object, manager, sessionMock.Object, InitialiseOptions().Object);
            var request = new Data.Student { Name = "Bob", Password = "password" };

            // Act
            await controller.Post(request);

            // Assert
            var command = Assert.IsType<StartSessionCommand>(manager.LastCommand);
            Assert.Equal("Bob", command.Name);
            Assert.Equal("password", command.Password);
            Assert.Equal(UserRole.Student, command.Role);
        }

        [Fact]
        public async Task PostChecksForInput()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionController>>();
            var manager = new FakeCommandManager();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new SessionController(loggerMock.Object, manager, sessionMock.Object, InitialiseOptions().Object);

            // Act
            var response = await controller.Post(null);

            // Assert
            var actual = Assert.IsType<ActionResult<Data.ExecutionResult<Data.Session>>>(response);
            Assert.IsType<BadRequestObjectResult>(actual.Result);
        }

        [Fact]
        public async Task PostFailsExecution()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing()
                .SetupApplyError("Something failed");
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new SessionController(loggerMock.Object, manager, sessionMock.Object, InitialiseOptions().Object);
            var request = new Data.Student { Name = "Bob" };

            // Act
            var response = await controller.Post(request);

            // Assert
            var actual = Assert.IsType<ActionResult<Data.ExecutionResult<Data.Session>>>(response);
            var objectResult = Assert.IsType<ObjectResult>(actual.Result);
            Assert.Equal(500, objectResult.StatusCode);
            var innerResponse = Assert.IsType<Data.ExecutionResult<Data.Session>>(objectResult.Value);
            Assert.Null(innerResponse.ValidationErrors);
            Assert.NotEmpty(innerResponse.ExecutionErrors);
        }

        [Fact]
        public async Task PostFailsValidation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing()
                .SetupValidateErrors("Oops");
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new SessionController(loggerMock.Object, manager, sessionMock.Object, InitialiseOptions().Object);
            var request = new Data.Student();

            // Act
            var response = await controller.Post(request);

            // Assert
            var actual = Assert.IsType<ActionResult<Data.ExecutionResult<Data.Session>>>(response);
            var badRequest = Assert.IsType<BadRequestObjectResult>(actual.Result);
            var innerResponse = Assert.IsType<Data.ExecutionResult<Data.Session>>(badRequest.Value);
            Assert.NotEmpty(innerResponse.ValidationErrors);
            Assert.Null(innerResponse.ExecutionErrors);
        }

        private static Mock<IOptions<AppSettings>> InitialiseOptions()
        {
            var optsMock = new Mock<IOptions<AppSettings>>();
            optsMock.Setup(o => o.Value).Returns(new AppSettings());
            return optsMock;
        }
    }
}