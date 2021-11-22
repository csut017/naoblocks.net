using NaoBlocks.Common;
using NaoBlocks.Engine.Commands;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class BatchTests
    {
        [Fact]
        public async Task ValidateChecksChildren()
        {
            var child = new FakeCommand
            {
                OnValidation = () => new[] { new CommandError(1, "Failed") }
            };
            var command = new Batch(child);
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.True(child.ValidateCalled);
            Assert.Equal(new[] { "Failed" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ExecuteExecutesChildren()
        {
            var child = new FakeCommand();
            var command = new Batch(child);
            var engine = new FakeEngine();
            await engine.ExecuteAsync(command);
            Assert.True(child.ExecuteCalled);
        }
    }
}
