using NaoBlocks.Engine.Commands;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class CompileCodeTests
    {
        [Fact]
        public async Task ValidationFailsWithNoCode()
        {
            var command = new CompileCode
            {
                Code = string.Empty
            };
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "No code to compile" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesWithSomeCode()
        {
            var command = new CompileCode
            {
                Code = "say()"
            };
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ExecuteCompilesCode()
        {
            var command = new CompileCode
            {
                Code = "say()"
            };
            var engine = new FakeEngine();
            var result = await engine.ExecuteAsync(command);
            Assert.True(result.WasSuccessful);
        }
    }
}
