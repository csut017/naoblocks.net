using Xunit;

namespace NaoBlocks.Communications.Tests
{
    public class ResultTests
    {
        [Fact]
        public void FailsSetsException()
        {
            var ex = new Exception("error");
            var result = Result.Fail<string>(ex);
            Assert.False(result.Success);
            Assert.Equal(ex, result.Error);
        }

        [Fact]
        public void OkSetsValues()
        {
            var result = Result.Ok("testing");
            Assert.True(result.Success);
            Assert.Equal("testing", result.Value);
        }
    }
}