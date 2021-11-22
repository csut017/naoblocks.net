using NaoBlocks.Engine.Commands;
using System;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class BatchCommandResultTests
    {
        [Fact]
        public void ConstructorChecksForErrors()
        {
            var results = new[]
            {
                new CommandResult(1, "Error")
            };
            var result = new BatchCommandResult(2, results);
            Assert.False(result.WasSuccessful);
            Assert.Equal("One or more commands failed", result.Error);
            Assert.Equal(2, result.Number);
        }

        [Fact]
        public void ToErrorsHandlesNoErrors()
        {
            var result = new BatchCommandResult(1, Array.Empty<CommandResult>());
            var errors = result.ToErrors();
            Assert.Empty(errors);
        }

        [Fact]
        public void ToErrorsHandlesErrors()
        {
            var results = new[]
            {
                new CommandResult(1, "Error")
            };
            var result = new BatchCommandResult(2, results);
            var errors = result.ToErrors();
            Assert.Equal(new [] {"Error"}, FakeEngine.GetErrors(errors));
        }
    }
}
