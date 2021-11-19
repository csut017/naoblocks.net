using Xunit;

namespace NaoBlocks.Common.Tests
{
    public class AstNodeTests
    {
        private const string numberValue = "123";
        private const string constantValue = "ConstantValue";
        private const string sourceIdValue = "ID1";

        [Fact]
        public void CanBeInitialised()
        {
            var node = new AstNode(AstNodeType.Constant, 
                new Token(TokenType.Number, numberValue),
                sourceIdValue);
            Assert.Equal(AstNodeType.Constant, node.Type);
            Assert.Equal(numberValue, node.Token.Value);
            Assert.Equal(sourceIdValue, node.SourceId);
        }

        [Theory]
        [InlineData(AstNodeType.Constant, TokenType.Constant, constantValue, sourceIdValue, "Constant:ConstantValue")]
        [InlineData(AstNodeType.Function, TokenType.Identifier, numberValue, "", "Function:123")]
        public void GeneratesHumanReadableString(AstNodeType nodeType, TokenType tokenType, string value, string sourceId, string expected)
        {
            var node = new AstNode(nodeType,
                new Token(tokenType, value),
                sourceId);
            Assert.Equal(expected, node.ToString());
        }

        [Theory]
        [InlineData(true, "Function:Test(Constant:123)")]
        [InlineData(false, "Function:Test()")]
        public void ToStringHandlesArguments(bool include, string expected)
        {
            var node = new AstNode(AstNodeType.Function,
                new Token(TokenType.Identifier, "Test"),
                string.Empty);
            node.Arguments.Add(new AstNode(AstNodeType.Constant, new Token(TokenType.Number, numberValue), string.Empty));
            var opts = new AstNode.DisplayOptions
            {
                ExcludeArguments = !include
            };
            Assert.Equal(expected, node.ToString(opts));
        }

        [Theory]
        [InlineData(true, "Function:Test{Function:Child}")]
        [InlineData(false, "Function:Test{}")]
        public void ToStringHandlesChildren(bool include, string expected)
        {
            var node = new AstNode(AstNodeType.Function,
                new Token(TokenType.Identifier, "Test"),
                string.Empty);
            node.Children.Add(new AstNode(AstNodeType.Function, new Token(TokenType.Identifier, "Child"), string.Empty));
            var opts = new AstNode.DisplayOptions
            {
                ExcludeChildren = !include
            };
            Assert.Equal(expected, node.ToString(opts));
        }
    }
}