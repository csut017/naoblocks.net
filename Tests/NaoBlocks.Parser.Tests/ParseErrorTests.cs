using NaoBlocks.Common;
using Xunit;

namespace NaoBlocks.Parser.Tests
{
    public class ParseErrorTests
    {
        [Fact]
        public void ConstructorSetsProperties()
        {
            var token = new Token(TokenType.Empty, string.Empty, 12, 34);
            var error = new ParseError("Testing", token);
            Assert.Equal("Testing", error.Message);
            Assert.Equal(12, error.LineNumber);
            Assert.Equal(34, error.LinePosition);
        }

        [Fact]
        public void ToStringReturnsExpectedString()
        {
            var token = new Token(TokenType.Empty, string.Empty, 12, 34);
            var error = new ParseError("Testing", token);
            Assert.Equal("Testing [12:34]", error.ToString());
        }
    }
}