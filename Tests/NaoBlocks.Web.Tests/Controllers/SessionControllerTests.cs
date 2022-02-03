using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Configuration;
using NaoBlocks.Web.Controllers;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

using Data = NaoBlocks.Engine.Data;
using Generators = NaoBlocks.Engine.Generators;
using Transfer = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class SessionControllerTests
    {
        [Fact]
        public async Task DeleteCallsCommand()
        {
            // Arrange
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<FinishSession>();
            var config = DefineSecuritySettings();
            var controller = new SessionController(loggerMock, engine, config.Object);
            engine.ConfigureUser(controller, "Mia", Data.UserRole.Teacher);

            // Act
            var response = await controller.Delete();

            // Assert
            Assert.True(response?.Value?.Successful, "Expected command to succeed");
            engine.Verify();
            var command = Assert.IsType<FinishSession>(engine.LastCommand);
            Assert.Equal("Mia", command.UserName);
        }

        [Fact]
        public async Task DeleteHandlesMissingUser()
        {
            // Arrange
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<FinishSession>();
            var config = DefineSecuritySettings();
            var controller = new SessionController(loggerMock, engine, config.Object);

            // Act
            var response = await controller.Delete();

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task PutCallsCommand()
        {
            // Arrange
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<RenewSession>();
            var config = DefineSecuritySettings();
            var controller = new SessionController(loggerMock, engine, config.Object);
            engine.ConfigureUser(controller, "Mia", Data.UserRole.Teacher);

            // Act
            var response = await controller.Put();

            // Assert
            Assert.True(response?.Value?.Successful, "Expected command to succeed");
            engine.Verify();
            var command = Assert.IsType<RenewSession>(engine.LastCommand);
            Assert.Equal("Mia", command.UserName);
        }

        [Fact]
        public async Task PutHandlesMissingUser()
        {
            // Arrange
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<FinishSession>();
            var config = DefineSecuritySettings();
            var controller = new SessionController(loggerMock, engine, config.Object);

            // Act
            var response = await controller.Put();

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task PostSettingsCallsCommand()
        {
            // Arrange
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<StoreSettings>(
                CommandResult.New(1, new Data.UserSettings()));

            var query = new Mock<RobotTypeData>();
            engine.RegisterQuery(query.Object);
            Data.RobotType? type = null;
            query.Setup(q => q.RetrieveDefaultAsync())
                .Returns(Task.FromResult(type));

            var config = DefineSecuritySettings();
            var controller = new SessionController(loggerMock, engine, config.Object);
            engine.ConfigureUser(controller, "Mia", Data.UserRole.Teacher);

            // Act
            var settings = new Transfer.EditorSettings();
            var response = await controller.PostSettings(settings);

            // Assert
            Assert.True(response?.Value?.Successful, "Expected command to succeed");
            engine.Verify();
            var command = Assert.IsType<StoreSettings>(engine.LastCommand);
            Assert.Equal("Mia", command.UserName);
        }

        [Fact]
        public async Task PostSettingsHandlesMissingUser()
        {
            // Arrange
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<FinishSession>();
            var config = DefineSecuritySettings();
            var controller = new SessionController(loggerMock, engine, config.Object);

            // Act
            var settings = new Transfer.EditorSettings();
            var response = await controller.PostSettings(settings);

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task PostSettingsHandlesMissingData()
        {
            // Arrange
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<FinishSession>();
            var config = DefineSecuritySettings();
            var controller = new SessionController(loggerMock, engine, config.Object);

            // Act
            var response = await controller.PostSettings(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Theory]
        [InlineData(4, 5)]
        [InlineData(9, 0)]
        [InlineData(15, 0)]
        public async Task GetRetrievesUser(int minute, int timeRemaining)
        {
            // Arrange
            var now = new DateTime(2100, 1, 2, 3, minute, 5);
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine();
            var query = new Mock<SessionData>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveForUserAsync(It.IsAny<Data.User>()))
                .Returns(Task.FromResult((Data.Session?)new Data.Session
                {
                    WhenExpires = new DateTime(2100, 1, 2, 3, 9, 5)
                }));
            var config = DefineSecuritySettings();
            var controller = new SessionController(loggerMock, engine, config.Object)
            {
                CurrentTimeFunc = () => now
            };
            engine.ConfigureUser(controller, "Mia", Data.UserRole.Teacher);

            // Act
            var response = await controller.Get();

            // Assert
            Assert.Equal("Mia", response?.Value?.Name);
            Assert.Equal(timeRemaining, response?.Value?.TimeRemaining);
        }

        [Fact]
        public async Task GetHandlesMissingSession()
        {
            // Arrange
            var now = new DateTime(2100, 1, 2, 3, 4, 5);
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine();
            var query = new Mock<SessionData>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveForUserAsync(It.IsAny<Data.User>()))
                .Returns(Task.FromResult((Data.Session?)null));
            var config = DefineSecuritySettings();
            var controller = new SessionController(loggerMock, engine, config.Object)
            {
                CurrentTimeFunc = () => now
            };
            engine.ConfigureUser(controller, "Mia", Data.UserRole.Teacher);

            // Act
            var response = await controller.Get();

            // Assert
            Assert.Equal("Mia", response?.Value?.Name);
            Assert.Equal(0, response?.Value?.TimeRemaining);
        }

        [Fact]
        public async Task GetHandlesMissingUser()
        {
            // Arrange
            var now = new DateTime(2100, 1, 2, 3, 4, 5);
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine();
            var config = DefineSecuritySettings();
            var controller = new SessionController(loggerMock, engine, config.Object)
            {
                CurrentTimeFunc = () => now
            };

            // Act
            var response = await controller.Get();

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task GetSettingsHandlesNoDefaultRobotType()
        {
            // Arrange
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine();
            var query = new Mock<RobotTypeData>();
            engine.RegisterQuery(query.Object);
            Data.RobotType? type = null;
            query.Setup(q => q.RetrieveDefaultAsync())
                .Returns(Task.FromResult(type));
            var config = DefineSecuritySettings();
            var controller = new SessionController(loggerMock, engine, config.Object);
            (var user, _) = engine.ConfigureUser(controller, "Mia", Data.UserRole.Teacher);

            // Act
            var response = await controller.GetSettings();

            // Assert
            Assert.False(response?.Value?.IsSystemInitialised, "System should not be initialised");
            Assert.Null(response?.Value?.Toolbox);
        }

        [Fact]
        public async Task GetSettingsHandlesDefaultRobotType()
        {
            // Arrange
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine();
            var query = new Mock<RobotTypeData>();
            engine.RegisterQuery(query.Object);
            var type = new Data.RobotType { };
            query.Setup(q => q.RetrieveDefaultAsync())
                .Returns(Task.FromResult((Data.RobotType?)type));
            var generator = new Mock<Generators.UserToolbox>();
            engine.RegisterGenerator(generator.Object);
            var result = Tuple.Create(GenerateToolbox(), string.Empty);
            generator.Setup(g => g.GenerateAsync(Engine.ReportFormat.Text))
                .Returns(Task.FromResult(result));
            var config = DefineSecuritySettings();
            var controller = new SessionController(loggerMock, engine, config.Object);
            (var user, _) = engine.ConfigureUser(controller, "Mia", Data.UserRole.Teacher);

            // Act
            var response = await controller.GetSettings();

            // Assert
            Assert.True(response?.Value?.IsSystemInitialised, "System should be initialised");
            Assert.Equal("<toolbox/>", response?.Value?.Toolbox);
        }

        [Fact]
        public async Task GetSettingsHandlesConfiguredRobotType()
        {
            // Arrange
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine();
            var query = new Mock<RobotTypeData>();
            engine.RegisterQuery(query.Object);
            var type = new Data.RobotType { };
            query.Setup(q => q.RetrieveByIdAsync("types/1"))
                .Returns(Task.FromResult((Data.RobotType?)type));
            var generator = new Mock<Generators.UserToolbox>();
            engine.RegisterGenerator(generator.Object);
            var result = Tuple.Create(GenerateToolbox(), string.Empty);
            generator.Setup(g => g.GenerateAsync(Engine.ReportFormat.Text))
                .Returns(Task.FromResult(result));
            var config = DefineSecuritySettings();
            var controller = new SessionController(loggerMock, engine, config.Object);
            (var user, _) = engine.ConfigureUser(controller, "Mia", Data.UserRole.Teacher);
            user.Settings.RobotTypeId = "types/1";

            // Act
            var response = await controller.GetSettings();

            // Assert
            Assert.True(response?.Value?.IsSystemInitialised, "System should be initialised");
            Assert.Equal("<toolbox/>", response?.Value?.Toolbox);
        }

        private static Stream GenerateToolbox(string data = "<toolbox/>")
        {
            var stream = new MemoryStream(
                Encoding.UTF8.GetBytes(data));
            return stream;
        }

        [Fact]
        public async Task GetSettingsHandlesMissingUser()
        {
            // Arrange
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine();
            var config = DefineSecuritySettings();
            var controller = new SessionController(loggerMock, engine, config.Object);

            // Act
            var response = await controller.GetSettings();

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        private static Mock<IOptions<Security>> DefineSecuritySettings(string secret = "Testing-1234567890")
        {
            var config = new Mock<IOptions<Security>>();
            var security = new Mock<Security>();
            security.Object.JwtSecret = secret;
            config.Setup(c => c.Value).Returns(security.Object);
            return config;
        }

        [Fact]
        public async Task PostHandlesMissingData()
        {
            // Arrange
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<FinishSession>();
            var config = DefineSecuritySettings();
            var controller = new SessionController(loggerMock, engine, config.Object);

            // Act
            var response = await controller.Post(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(response?.Result);
        }

        [Fact]
        public async Task PostStartsRobotSession()
        {
            // Arrange
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<StartRobotSession>(
                CommandResult.New(1, new Data.Session
                {
                    IsRobot = true,
                    WhenExpires = new DateTime(2100, 12, 30, 13, 0, 0),
                    UserId = "robots/1",
                    Id = "sessions/10"
                }));
            var config = DefineSecuritySettings();
            var controller = new SessionController(loggerMock, engine, config.Object)
            {
                CurrentTimeFunc = () => new DateTime(2100, 12, 30, 12, 0, 0)
            };

            // Act
            var request = new Transfer.User
            {
                Role = "robot",
                Name = "karetao",
                Password = "1234"
            };
            var response = await controller.Post(request);

            // Assert
            engine.Verify();
            var data = Assert.IsType<ExecutionResult<UserSessionResult>>(response?.Value);
            Assert.Equal("Robot", data.Output?.Role);
            Assert.Equal(60, data.Output?.TimeRemaining);
            Assert.False(string.IsNullOrEmpty(data.Output?.Token), "Expected a token");
        }

        [Fact]
        public async Task PostHandlesRobotValidationErrors()
        {
            // Arrange
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine
            {
                OnValidate = c => new[]
                {
                    new CommandError(1, "error")
                }
            };
            var config = DefineSecuritySettings();
            var controller = new SessionController(loggerMock, engine, config.Object)
            {
                CurrentTimeFunc = () => new DateTime(2100, 12, 30, 12, 0, 0)
            };

            // Act
            var request = new Transfer.User
            {
                Role = "robot",
                Name = "karetao",
                Password = "1234"
            };
            var response = await controller.Post(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(response?.Result);
        }

        [Fact]
        public async Task PostHandlesRobotExecutionErrors()
        {
            // Arrange
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<StartRobotSession>(
                new CommandResult(2, "error"));
            var config = DefineSecuritySettings();
            var controller = new SessionController(loggerMock, engine, config.Object)
            {
                CurrentTimeFunc = () => new DateTime(2100, 12, 30, 12, 0, 0)
            };

            // Act
            var request = new Transfer.User
            {
                Role = "robot",
                Name = "karetao",
                Password = "1234"
            };
            var response = await controller.Post(request);

            // Assert
            var result = Assert.IsType<ObjectResult>(response?.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
            var data = Assert.IsType<ExecutionResult<UserSessionResult>>(result.Value);
            Assert.NotEmpty(data.ExecutionErrors);
            Assert.False(data.Successful);
        }

        [Fact]
        public async Task PostStartsUserSession()
        {
            // Arrange
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<StartUserSession>(
                CommandResult.New(1, new Data.Session
                {
                    IsRobot = false,
                    WhenExpires = new DateTime(2100, 12, 30, 13, 0, 0),
                    UserId = "users/1",
                    Id = "sessions/10",
                    Role = Data.UserRole.Student
                }));
            var config = DefineSecuritySettings();
            var controller = new SessionController(loggerMock, engine, config.Object)
            {
                CurrentTimeFunc = () => new DateTime(2100, 12, 30, 12, 0, 0)
            };

            // Act
            var request = new Transfer.User
            {
                Role = "Student",
                Name = "karetao",
                Password = "1234"
            };
            var response = await controller.Post(request);

            // Assert
            engine.Verify();
            var data = Assert.IsType<ExecutionResult<UserSessionResult>>(response?.Value);
            Assert.Equal("Student", data.Output?.Role);
            Assert.Equal(60, data.Output?.TimeRemaining);
            Assert.False(string.IsNullOrEmpty(data.Output?.Token), "Expected a token");
        }

        [Fact]
        public async Task PostHandlesUserValidationErrors()
        {
            // Arrange
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine
            {
                OnValidate = c => new[]
                {
                    new CommandError(1, "error")
                }
            };
            var config = DefineSecuritySettings();
            var controller = new SessionController(loggerMock, engine, config.Object)
            {
                CurrentTimeFunc = () => new DateTime(2100, 12, 30, 12, 0, 0)
            };

            // Act
            var request = new Transfer.User
            {
                Role = "student",
                Name = "karetao",
                Password = "1234"
            };
            var response = await controller.Post(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(response?.Result);
        }

        [Fact]
        public async Task PostHandlesUserExecutionErrors()
        {
            // Arrange
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<StartUserSession>(
                new CommandResult(2, "error"));
            var config = DefineSecuritySettings();
            var controller = new SessionController(loggerMock, engine, config.Object)
            {
                CurrentTimeFunc = () => new DateTime(2100, 12, 30, 12, 0, 0)
            };

            // Act
            var request = new Transfer.User
            {
                Role = "student",
                Name = "karetao",
                Password = "1234"
            };
            var response = await controller.Post(request);

            // Assert
            var result = Assert.IsType<ObjectResult>(response?.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
            var data = Assert.IsType<ExecutionResult<UserSessionResult>>(result.Value);
            Assert.NotEmpty(data.ExecutionErrors);
            Assert.False(data.Successful);
        }

        [Fact]
        public async Task PostStartsTokenSession()
        {
            // Arrange
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<StartUserSessionViaToken>(
                CommandResult.New(1, new Data.Session
                {
                    IsRobot = false,
                    WhenExpires = new DateTime(2100, 12, 30, 13, 0, 0),
                    UserId = "users/1",
                    Id = "sessions/10",
                    Role = Data.UserRole.Teacher
                }));
            var config = DefineSecuritySettings();
            var controller = new SessionController(loggerMock, engine, config.Object)
            {
                CurrentTimeFunc = () => new DateTime(2100, 12, 30, 12, 0, 0)
            };

            // Act
            var request = new Transfer.User
            {
                Token = "1234"
            };
            var response = await controller.Post(request);

            // Assert
            engine.Verify();
            var data = Assert.IsType<ExecutionResult<UserSessionResult>>(response?.Value);
            Assert.Equal("Teacher", data.Output?.Role);
            Assert.Equal(60, data.Output?.TimeRemaining);
            Assert.False(string.IsNullOrEmpty(data.Output?.Token), "Expected a token");
        }

        [Fact]
        public async Task PostHandlesTokenValidationErrors()
        {
            // Arrange
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine
            {
                OnValidate = c => new[]
                {
                    new CommandError(1, "error")
                }
            };
            var config = DefineSecuritySettings();
            var controller = new SessionController(loggerMock, engine, config.Object)
            {
                CurrentTimeFunc = () => new DateTime(2100, 12, 30, 12, 0, 0)
            };

            // Act
            var request = new Transfer.User
            {
                Role = "student",
                Name = "karetao",
                Password = "1234"
            };
            var response = await controller.Post(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(response?.Result);
        }

        [Fact]
        public async Task PostHandlesTokenExecutionErrors()
        {
            // Arrange
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<StartUserSession>(
                new CommandResult(2, "error"));
            var config = DefineSecuritySettings();
            var controller = new SessionController(loggerMock, engine, config.Object)
            {
                CurrentTimeFunc = () => new DateTime(2100, 12, 30, 12, 0, 0)
            };

            // Act
            var request = new Transfer.User
            {
                Role = "student",
                Name = "karetao",
                Password = "1234"
            };
            var response = await controller.Post(request);

            // Assert
            var result = Assert.IsType<ObjectResult>(response?.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
            var data = Assert.IsType<ExecutionResult<UserSessionResult>>(result.Value);
            Assert.NotEmpty(data.ExecutionErrors);
            Assert.False(data.Successful);
        }
    }
}