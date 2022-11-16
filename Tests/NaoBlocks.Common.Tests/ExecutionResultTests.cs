
using System.Collections.Generic;
using Xunit;

namespace NaoBlocks.Common.Tests
{
    public class ExecutionResultTests
    {
        private const string dataValue = "Testing";

        [Fact]
        public void SuccessfulHandlesNoErrors()
        {
            var result = new ExecutionResult();
            Assert.True(result.Successful);
        }

        [Fact]
        public void SuccessfulHandlesValdiationErrors()
        {
            var result = new ExecutionResult
            {
                ValidationErrors = new List<CommandError> { new CommandError(0, "Testing") }
            };
            Assert.False(result.Successful);
        }

        [Fact]
        public void SuccessfulHandlesExecutionErrors()
        {
            var result = new ExecutionResult
            {
                ExecutionErrors = new List<CommandError> { new CommandError(0, "Testing") }
            };
            Assert.False(result.Successful);
        }

        [Fact]
        public void NewGenerateDataCarryingResult()
        {
            var result = ExecutionResult.New(dataValue);
            Assert.Equal(dataValue, result.Output);
        }
    }
}
