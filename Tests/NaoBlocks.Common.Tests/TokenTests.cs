using Xunit;

namespace NaoBlocks.Common.Tests
{
    public class TokenTests
    {
        private const string numberValue = "123";
        private const string constantValue = "ConstantValue";

        [Fact]
        public void CanBeInitialised()
        {
            var token = new Token(TokenType.Number, numberValue);
            Assert.Equal(TokenType.Number, token.Type);
            Assert.Equal("123", token.Value);
        }

        [Theory]
        [InlineData(TokenType.Constant, constantValue, "{ConstantValue:Constant}")]
        [InlineData(TokenType.Number, numberValue, "{123:Number}")]
        public void GeneratesHumanReadableString(TokenType type, string value, string expected)
        {
            var token = new Token(type, value);
            Assert.Equal(expected, token.ToString());
        }
    }
}