using Xunit;

namespace NaoBlocks.Parser.Tests
{
    public class TokenTests
    {
        [Fact]
        public void ConvertsToString()
        {
            var token = new Token(TokenType.Constant, "TEST");
            const string expected = "{TEST:Constant}";
            Assert.Equal(expected, token.ToString());
        }
    }
}