using Microsoft.Extensions.Options;
using Moq;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Web.Configuration;
using NaoBlocks.Web.Controllers;
using System.Threading.Tasks;
using Xunit;

using Data = NaoBlocks.Engine.Data;

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