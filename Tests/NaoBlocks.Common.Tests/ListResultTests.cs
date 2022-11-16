
using Xunit;

namespace NaoBlocks.Common.Tests
{
    public class ListResultTests
    {
        [Fact]
        public void NewGeneratesResult()
        {
            var data = new string[] { "1", "Two", "Toru" };
            var result = ListResult.New(data);
            Assert.Equal(data, result.Items);
            Assert.Equal(3, result.Count);
            Assert.Equal(0, result.Page);
        }

        [Fact]
        public void NewHandlesOptionalArguments()
        {
            var data = new string[] { "1", "Two", "Toru" };
            var result = ListResult.New(data, 5, 2);
            Assert.Equal(data, result.Items);
            Assert.Equal(5, result.Count);
            Assert.Equal(2, result.Page);
        }
    }
}
