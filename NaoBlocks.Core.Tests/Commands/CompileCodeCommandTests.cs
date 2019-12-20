﻿using NaoBlocks.Core.Commands;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Core.Tests.Commands
{
    public class CompileCodeCommandTests
    {
        [Fact]
        public async Task CompilesCode()
        {
            var command = new CompileCodeCommand { Code = "rest()" };
            var result = await command.ApplyAsync(null);
            Assert.True(result.WasSuccessful);
            Assert.Null(command.Output.Errors);
            Assert.NotEmpty(command.Output.Nodes);
        }

        [Fact]
        public async Task FailsValidationWithNoCode()
        {
            var command = new CompileCodeCommand { Code = string.Empty };
            var result = await command.ValidateAsync(null);
            var expected = new[]
            {
                "No code to compile"
            };
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ValidatesWithCode()
        {
            var command = new CompileCodeCommand { Code = "rest()" };
            var result = await command.ValidateAsync(null);
            Assert.Empty(result);
        }
    }
}