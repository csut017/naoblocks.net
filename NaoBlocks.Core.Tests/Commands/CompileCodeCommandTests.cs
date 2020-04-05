using NaoBlocks.Core.Commands;
using NaoBlocks.Core.Models;
using System.Linq;
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
            var result = (await command.ApplyAsync(null)).As<CompiledCodeProgram>();
            Assert.True(result.WasSuccessful);
            Assert.Null(result.Output?.Errors);
            Assert.NotEmpty(result.Output?.Nodes);
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
            Assert.Equal(expected, result.Select(r => r.Error));
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