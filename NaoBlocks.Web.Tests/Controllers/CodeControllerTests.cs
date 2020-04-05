using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NaoBlocks.Core.Models;
using NaoBlocks.Web.Controllers;
using Raven.Client.Documents.Session;
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
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new CodeController(loggerMock.Object, manager, sessionMock.Object);

            // Act
            var response = await controller.Compile(null);

            // Assert
            var actual = Assert.IsType<ActionResult<Data.ExecutionResult<CompiledCodeProgram>>>(response);
            Assert.IsType<BadRequestObjectResult>(actual.Result);
        }

        [Fact]
        public async Task CompileCompilesCode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CodeController>>();
            var manager = new FakeCommandManager();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new CodeController(loggerMock.Object, manager, sessionMock.Object);
            var request = new Data.CodeProgram { Code = "rest()" };

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
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new CodeController(loggerMock.Object, manager, sessionMock.Object);
            var request = new Data.CodeProgram { Code = "test" };

            // Act
            var response = await controller.Compile(request);

            // Assert
            var actual = Assert.IsType<ActionResult<Data.ExecutionResult<CompiledCodeProgram>>>(response);
            var objectResult = Assert.IsType<ObjectResult>(actual.Result);
            Assert.Equal(500, objectResult.StatusCode);
            var innerResponse = Assert.IsType<Data.ExecutionResult<CompiledCodeProgram>>(objectResult.Value);
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
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var controller = new CodeController(loggerMock.Object, manager, sessionMock.Object);
            var request = new Data.CodeProgram();

            // Act
            var response = await controller.Compile(request);

            // Assert
            var actual = Assert.IsType<ActionResult<Data.ExecutionResult<CompiledCodeProgram>>>(response);
            var badRequest = Assert.IsType<BadRequestObjectResult>(actual.Result);
            var innerResponse = Assert.IsType<Data.ExecutionResult<CompiledCodeProgram>>(badRequest.Value);
            Assert.NotEmpty(innerResponse.ValidationErrors);
            Assert.Null(innerResponse.ExecutionErrors);
            Assert.Null(innerResponse.Output);
        }
    }
}