using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NaoBlocks.Web.Controllers;
using NaoBlocks.Web.Helpers;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class CodeControllerTests
    {
        [Fact]
        public async Task CompileCompilesCode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CodeController>>();
            var manager = new FakeCommandManager();
            var controller = new CodeController(loggerMock.Object, manager);
            var request = new Dtos.RobotCode { Program = "rest()" };

            // Act
            var response = await controller.Compile(request);

            // Assert
            Assert.Null(response.Value.ValidationErrors);
            Assert.Null(response.Value.ExecutionErrors);
            Assert.Empty(response.Value.Output.Errors);
            Assert.NotEmpty(response.Value.Output.Nodes);
        }

        [Fact]
        public async Task CompileFailsExecution()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CodeController>>();
            var manager = new FakeCommandManager
            {
                ApplyCommand = c => Task.FromResult(new CommandResult(1, "Something failed"))
            };
            var controller = new CodeController(loggerMock.Object, manager);
            var request = new Dtos.RobotCode { Program = "test" };

            // Act
            var response = await controller.Compile(request);

            // Assert
            var actual = Assert.IsType<ActionResult<Dtos.ExecutionResult<Dtos.RobotCodeCompilation>>>(response);
            var objectResult = Assert.IsType<ObjectResult>(actual.Result);
            Assert.Equal(500, objectResult.StatusCode);
            var innerResponse = Assert.IsType<Dtos.ExecutionResult<Dtos.RobotCodeCompilation>>(objectResult.Value);
            Assert.Null(innerResponse.ValidationErrors);
            Assert.NotEmpty(innerResponse.ExecutionErrors);
            Assert.Null(innerResponse.Output);
        }

        [Fact]
        public async Task CompileFailsValidation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CodeController>>();
            var manager = new FakeCommandManager();
            var controller = new CodeController(loggerMock.Object, manager);
            var request = new Dtos.RobotCode();

            // Act
            var response = await controller.Compile(request);

            // Assert
            var actual = Assert.IsType<ActionResult<Dtos.ExecutionResult<Dtos.RobotCodeCompilation>>>(response);
            var badRequest = Assert.IsType<BadRequestObjectResult>(actual.Result);
            var innerResponse = Assert.IsType<Dtos.ExecutionResult<Dtos.RobotCodeCompilation>>(badRequest.Value);
            Assert.NotEmpty(innerResponse.ValidationErrors);
            Assert.Null(innerResponse.ExecutionErrors);
            Assert.Null(innerResponse.Output);
        }
    }
}