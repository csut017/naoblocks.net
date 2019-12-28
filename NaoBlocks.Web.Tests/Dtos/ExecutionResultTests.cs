using NaoBlocks.Core.Commands;
using NaoBlocks.Web.Dtos;
using Xunit;

namespace NaoBlocks.Web.Tests.Dtos
{
    public class ExecutionResultTests
    {
        [Fact]
        public void SuccessfulIsFalseWithExecutionErrors()
        {
            var result = new ExecutionResult
            {
                ExecutionErrors = new[] { new CommandError(0, "Error") }
            };
            Assert.False(result.Successful);
        }

        [Fact]
        public void SuccessfulIsFalseWithValidationErrors()
        {
            var result = new ExecutionResult
            {
                ValidationErrors = new[] { new CommandError(0, "Error") }
            };
            Assert.False(result.Successful);
        }

        [Fact]
        public void SuccessfulIsTrueWithNoErrors()
        {
            var result = new ExecutionResult();
            Assert.True(result.Successful);
        }
    }
}