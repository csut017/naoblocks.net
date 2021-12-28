using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
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

        [Theory]
        [InlineData(4, 5)]
        [InlineData(9, 0)]
        [InlineData(15, 0)]
        public async Task GetRetrievesUser(int minute, int timeRemaining)
        {
            // Arrange
            var now = new DateTime(2021, 1, 2, 3, minute, 5);
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine();
            var query = new Mock<SessionData>();
            engine.RegisterQuery(query.Object);
            query.Setup<Task<Data.Session?>>(q => q.RetrieveForUserAsync(It.IsAny<Data.User>()))
                .Returns(Task.FromResult<Data.Session?>((Data.Session?)new Data.Session
                {
                    WhenExpires = new DateTime(2021, 1, 2, 3, 9, 5)
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
            var now = new DateTime(2021, 1, 2, 3, 4, 5);
            var loggerMock = new FakeLogger<SessionController>();
            var engine = new FakeEngine();
            var query = new Mock<SessionData>();
            engine.RegisterQuery(query.Object);
            query.Setup<Task<Data.Session?>>(q => q.RetrieveForUserAsync(It.IsAny<Data.User>()))
                .Returns(Task.FromResult<Data.Session?>((Data.Session?)null));
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
            var now = new DateTime(2021, 1, 2, 3, 4, 5);
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

        private static Mock<IOptions<Security>> DefineSecuritySettings(string secret = "Testing")
        {
            var config = new Mock<IOptions<Security>>();
            var security = new Mock<Security>();
            security.Object.JwtSecret = secret;
            config.Setup(c => c.Value).Returns(security.Object);
            return config;
        }
    }
}