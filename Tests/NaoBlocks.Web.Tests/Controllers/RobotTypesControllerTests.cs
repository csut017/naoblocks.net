using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Controllers;
using System.Threading.Tasks;
using Xunit;

using Data = NaoBlocks.Engine.Data;
using Transfer = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class RobotTypesControllerTests
    {
        [Fact]
        public async Task DeleteCallsDelete()
        {
            // Arrange
            var logger = new FakeLogger<RobotTypesController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<DeleteRobotType>();
            Mock<IWebHostEnvironment> env = InitialiseEnvironment();
            var controller = new RobotTypesController(
                logger,
                engine,
                env.Object);

            // Act
            var response = await controller.Delete("karetao");

            // Assert
            var result = Assert.IsType<ExecutionResult>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();
        }

        /*
        [Fact]
        public async Task GetRetrievesUserViaQuery()
        {
            // Arrange
            var logger = new FakeLogger<RobotTypesController>();
            var engine = new FakeEngine();
            var controller = new RobotTypesController(
                logger,
                engine);
            (_, var userQuery) = engine.ConfigureUser(controller, "Mia", Data.UserRole.Teacher);
            userQuery.Setup(q => q.RetrieveByNameAsync("Moana"))
                .Returns(Task.FromResult((Data.User?)new Data.User { Name = "Moana" }));
            var codeQuery = new Mock<CodeData>();
            codeQuery.Setup(q => q.RetrieveCodeAsync("Moana", 3))
                .Returns(Task.FromResult((Data.CodeProgram?)new Data.CodeProgram { Name = "hōtaka", Code = "go()" }));
            engine.RegisterQuery(codeQuery.Object);

            // Act
            var response = await controller.Get("3", "Moana");

            // Assert
            Assert.NotNull(response.Value);
            Assert.Equal("hōtaka", response.Value?.Name);
            Assert.Equal("go()", response.Value?.Code);
        }
        */

        private static Mock<IWebHostEnvironment> InitialiseEnvironment(string value = "http://test")
        {
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.WebRootPath).Returns(value);
            return env;
        }
    }
}
