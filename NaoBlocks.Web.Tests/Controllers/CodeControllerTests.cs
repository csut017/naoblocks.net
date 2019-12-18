using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NaoBlocks.Core.Models;
using NaoBlocks.Web.Controllers;
using System.Threading.Tasks;
using Xunit;

using Data = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class CodeControllerTests
    {
        [Fact]
        public async Task CompileChecksForInput()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CodeController>>();
            var manager = new FakeCommandManager();
            var controller = new CodeController(loggerMock.Object, manager);

            // Act
            var response = await controller.Compile(null);

            // Assert
            var actual = Assert.IsType<ActionResult<Data.ExecutionResult<RobotCodeCompilation>>>(response);
            Assert.IsType<BadRequestObjectResult>(actual.Result);
        }

        [Fact]
        public async Task CompileCompilesCode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CodeController>>();
            var manager = new FakeCommandManager();
            var controller = new CodeController(loggerMock.Object, manager);
            var request = new Data.RobotCode { Program = "rest()" };

            // Act
            var response = await controller.Compile(request);

            // Assert
            Assert.Null(response.Value.ValidationErrors);
            Assert.Null(response.Value.ExecutionErrors);
            Assert.Null(response.Value.Output.Errors);
            Assert.NotEmpty(response.Value.Output.Nodes);
        }

        [Fact]
        public async Task CompileFailsExecution()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CodeController>>();
            var manager = new FakeCommandManager()
                .SetupApplyError("Something failed");
            var controller = new CodeController(loggerMock.Object, manager);
            var request = new Data.RobotCode { Program = "test" };

            // Act
            var response = await controller.Compile(request);

            // Assert
            var actual = Assert.IsType<ActionResult<Data.ExecutionResult<RobotCodeCompilation>>>(response);
            var objectResult = Assert.IsType<ObjectResult>(actual.Result);
            Assert.Equal(500, objectResult.StatusCode);
            var innerResponse = Assert.IsType<Data.ExecutionResult<RobotCodeCompilation>>(objectResult.Value);
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
            var request = new Data.RobotCode();

            // Act
            var response = await controller.Compile(request);

            // Assert
            var actual = Assert.IsType<ActionResult<Data.ExecutionResult<RobotCodeCompilation>>>(response);
            var badRequest = Assert.IsType<BadRequestObjectResult>(actual.Result);
            var innerResponse = Assert.IsType<Data.ExecutionResult<RobotCodeCompilation>>(badRequest.Value);
            Assert.NotEmpty(innerResponse.ValidationErrors);
            Assert.Null(innerResponse.ExecutionErrors);
            Assert.Null(innerResponse.Output);
        }
    }
}