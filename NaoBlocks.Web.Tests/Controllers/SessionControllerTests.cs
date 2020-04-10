using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NaoBlocks.Core.Commands;
using NaoBlocks.Core.Models;
using NaoBlocks.Web.Controllers;
using NaoBlocks.Web.Helpers;
using Raven.Client.Documents.Session;
using RavenDB.Mocks;
using System;
using System.Threading.Tasks;
using Xunit;

using Data = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class SessionControllerTests
    {
        [Fact]
        public async Task DeleteExecutesCommand()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new SessionController(loggerMock.Object, manager, sessionMock.Object, InitialiseOptions().Object);
            Utils.InitialiseUser(sessionMock, controller, new User { Id = "users/1", Name = "Bob" });

            // Act
            await controller.Delete();

            // Assert
            var command = Assert.IsType<FinishSession>(manager.LastCommand);
            Assert.Equal("users/1", command.UserId);
        }

        [Fact]
        public async Task DeleteFailsIfUserNotSet()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new SessionController(loggerMock.Object, manager, sessionMock.Object, InitialiseOptions().Object);

            // Act
            var response = await controller.Delete();

            // Assert
            var actual = Assert.IsType<ActionResult<Data.ExecutionResult>>(response);
            Assert.IsType<NotFoundResult>(actual.Result);
        }

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
            var actual = Assert.IsType<ActionResult<Data.UserSession>>(response);
            Assert.IsType<NotFoundResult>(actual.Result);
        }

        [Fact]
        public async Task GetReturnsCurrentUser()
        {
            // Arrange
            var sessions = new[]
            {
                new Session { WhenExpires = new DateTime(2019, 1, 2)}
            };
            var loggerMock = new Mock<ILogger<SessionController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing()
                .SetupValidateErrors("Oops");
            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<Session>(null, null, false)).Returns(sessions.AsRavenQueryable());
            var controller = new SessionController(loggerMock.Object, manager, sessionMock.Object, InitialiseOptions().Object)
            {
                CurrentTimeFunc = () => new DateTime(2019, 1, 1)
            };
            Utils.InitialiseUser(sessionMock, controller, new User { Id = "users/1", Name = "Bob" });

            // Act
            var response = await controller.Get();

            // Assert
            var actual = Assert.IsType<ActionResult<Data.UserSession>>(response);
            var user = Assert.IsType<Data.UserSession>(actual.Value);
            Assert.Equal("Bob", user.Name);
            Assert.Equal(1440, user.TimeRemaining);
        }

        [Fact]
        public async Task PostCallsCorrectCommand()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing()
                .SetupApply(cmd => Task.FromResult(CommandResult.New(0, new Session
                {
                    WhenExpires = DateTime.UtcNow.AddDays(1)
                })));
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new SessionController(loggerMock.Object, manager, sessionMock.Object, InitialiseOptions().Object);
            var request = new Data.Student { Name = "Bob", Password = "password" };

            // Act
            await controller.Post(request);

            // Assert
            var command = Assert.IsType<StartUserSession>(manager.LastCommand);
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

        [Fact]
        public async Task PostReturnsCorrectData()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionController>>();
            var manager = new FakeCommandManager()
                .SetupDoNothing()
                .SetupApply(cmd => Task.FromResult(CommandResult.New(0, new Session
                {
                    Role = UserRole.Teacher,
                    WhenExpires = DateTime.UtcNow.AddDays(1)
                })));
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new SessionController(loggerMock.Object, manager, sessionMock.Object, InitialiseOptions().Object);
            var request = new Data.Student { Name = "Bob", Password = "password" };

            // Act
            var result = await controller.Post(request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Data.ExecutionResult<Data.Session>>>(result);
            var execResult = Assert.IsType<Data.ExecutionResult<Data.Session>>(actionResult.Value);
            Assert.False(string.IsNullOrEmpty(execResult.Output.Token));
            Assert.Equal(UserRole.Teacher.ToString(), execResult.Output.Role);
        }

        private static Mock<IOptions<AppSettings>> InitialiseOptions()
        {
            var optsMock = new Mock<IOptions<AppSettings>>();
            optsMock.Setup(o => o.Value).Returns(new AppSettings());
            return optsMock;
        }
    }
}