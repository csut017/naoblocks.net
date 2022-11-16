using Xunit;

namespace NaoBlocks.Common.Tests
{
    public class CommandErrorTests
    {
        private const string errorText = "Testing";

        [Fact]
        public void ConstructorStoresArguments()
        {
            var error = new CommandError(1, errorText);
            Assert.Equal(1, error.Number);
            Assert.Equal(errorText, error.Error);
        }
    }
}
