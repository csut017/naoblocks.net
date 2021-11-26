using Microsoft.AspNetCore.Mvc;
using Moq;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Web.Helpers;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class CommandExecutorTests
    {
        [Fact]
        public async Task ExecuteForHttpHandlesSuccess()
        {
            var engine = new FakeEngine();
            var controller = new ControllerShell();
            var response = await controller.ExecuteForHttp(engine);
            var result = Assert.IsType<ExecutionResult>(response.Value);
            Assert.True(result.Successful, "Expected the execution to pass");
        }

        [Fact]
        public async Task ExecuteForHttpHandlesValidationFailure()
        {
            var engine = new FakeEngine
            {
                OnValidate = c => new CommandError[] { new CommandError(1, "oops") }
            };
            var controller = new ControllerShell();
            var response = await controller.ExecuteForHttp(engine);
            var httpResult = Assert.IsType<BadRequestObjectResult>(response.Result);
            var result = Assert.IsType<ExecutionResult>(httpResult.Value);
            Assert.False(result.Successful, "Expected the execution to fail");
            Assert.Contains("oops", result.ValidationErrors.Select(e => e.Error));
            Assert.Empty(result.ExecutionErrors);
        }

        [Fact]
        public async Task ExecuteForHttpHandlesExecutionFailure()
        {
            var engine = new FakeEngine
            {
                OnExecute = c => new CommandResult(1, "no go")
            };
            var controller = new ControllerShell();
            var response = await controller.ExecuteForHttp(engine);
            var httpResult = Assert.IsType<ObjectResult>(response.Result);
            Assert.Equal(500, httpResult.StatusCode);
            var result = Assert.IsType<ExecutionResult>(httpResult.Value);
            Assert.False(result.Successful, "Expected the execution to fail");
            Assert.Empty(result.ValidationErrors);
            Assert.Contains("no go", result.ExecutionErrors.Select(e => e.Error));
        }

        private class ControllerShell: ControllerBase
        {
            public async Task<ActionResult<ExecutionResult>> ExecuteForHttp(IExecutionEngine engine)
            {
                var command = new Mock<CommandBase>();
                return await engine
                    .ExecuteForHttp(command.Object)
                    .ConfigureAwait(false);
            }
        }
    }
}
