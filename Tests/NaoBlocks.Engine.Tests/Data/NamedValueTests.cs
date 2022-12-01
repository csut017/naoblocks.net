using NaoBlocks.Engine.Data;
using Xunit;

namespace NaoBlocks.Engine.Tests.Data
{
    public class NamedValueTests
    {
        [Fact]
        public void NewGeneratesInstance()
        {
            var value = NamedValue.New("one", "tahi");
            Assert.Equal("one", value.Name);
            Assert.Equal("tahi", value.Value);
        }
    }
}