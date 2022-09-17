using Xunit;

namespace NaoBlocks.Common.Tests
{
    public class AstNodeTests
    {
        private const string constantValue = "ConstantValue";
        private const string numberValue = "123";
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
        [InlineData(AstNode.DisplayType.Include, "Function:Test(Constant:123)")]
        [InlineData(AstNode.DisplayType.Exclude, "Function:Test()")]
        [InlineData(AstNode.DisplayType.Ignore, "Function:Test")]
        public void ToStringHandlesArguments(AstNode.DisplayType displayType, string expected)
        {
            var node = new AstNode(AstNodeType.Function,
                new Token(TokenType.Identifier, "Test"),
                string.Empty);
            node.Arguments.Add(new AstNode(AstNodeType.Constant, new Token(TokenType.Number, numberValue), string.Empty));
            var opts = new AstNode.DisplayOptions
            {
                Arguments = displayType
            };
            Assert.Equal(expected, node.ToString(opts));
        }

        [Theory]
        [InlineData(AstNode.DisplayType.Include, AstNode.DisplayType.Include, "Function:Test(Constant:123){Function:Child}")]
        [InlineData(AstNode.DisplayType.Include, AstNode.DisplayType.Exclude, "Function:Test(Constant:123){}")]
        [InlineData(AstNode.DisplayType.Include, AstNode.DisplayType.Ignore, "Function:Test(Constant:123)")]
        [InlineData(AstNode.DisplayType.Exclude, AstNode.DisplayType.Include, "Function:Test(){Function:Child}")]
        [InlineData(AstNode.DisplayType.Exclude, AstNode.DisplayType.Exclude, "Function:Test(){}")]
        [InlineData(AstNode.DisplayType.Exclude, AstNode.DisplayType.Ignore, "Function:Test()")]
        [InlineData(AstNode.DisplayType.Ignore, AstNode.DisplayType.Include, "Function:Test{Function:Child}")]
        [InlineData(AstNode.DisplayType.Ignore, AstNode.DisplayType.Exclude, "Function:Test{}")]
        [InlineData(AstNode.DisplayType.Ignore, AstNode.DisplayType.Ignore, "Function:Test")]
        public void ToStringHandlesArgumentsAndChildren(AstNode.DisplayType argDisplay, AstNode.DisplayType childDisplay, string expected)
        {
            var node = new AstNode(AstNodeType.Function,
                new Token(TokenType.Identifier, "Test"),
                string.Empty);
            node.Arguments.Add(new AstNode(AstNodeType.Constant, new Token(TokenType.Number, numberValue), string.Empty));
            node.Children.Add(new AstNode(AstNodeType.Function, new Token(TokenType.Identifier, "Child"), string.Empty));
            var opts = new AstNode.DisplayOptions
            {
                Children = childDisplay,
                Arguments = argDisplay
            };
            Assert.Equal(expected, node.ToString(opts));
        }

        [Theory]
        [InlineData(AstNode.DisplayType.Include, "Function:Test{Function:Child}")]
        [InlineData(AstNode.DisplayType.Exclude, "Function:Test{}")]
        [InlineData(AstNode.DisplayType.Ignore, "Function:Test")]
        public void ToStringHandlesChildren(AstNode.DisplayType displayType, string expected)
        {
            var node = new AstNode(AstNodeType.Function,
                new Token(TokenType.Identifier, "Test"),
                string.Empty);
            node.Children.Add(new AstNode(AstNodeType.Function, new Token(TokenType.Identifier, "Child"), string.Empty));
            var opts = new AstNode.DisplayOptions
            {
                Children = displayType
            };
            Assert.Equal(expected, node.ToString(opts));
        }

        [Theory]
        [InlineData(true, "[ID1]Function:Test(Constant:123)")]
        [InlineData(false, "Function:Test(Constant:123)")]
        public void ToStringHandlesSourceID(bool include, string expected)
        {
            var node = new AstNode(AstNodeType.Function,
                new Token(TokenType.Identifier, "Test"),
                sourceIdValue);
            node.Arguments.Add(new AstNode(AstNodeType.Constant, new Token(TokenType.Number, numberValue), string.Empty));
            var opts = new AstNode.DisplayOptions
            {
                IncludeSourceIDs = include
            };
            Assert.Equal(expected, node.ToString(opts));
        }

        [Theory]
        [InlineData(true, "Function:Test=>IDENTIFIER(Constant:123=>NUMBER)")]
        [InlineData(false, "Function:Test(Constant:123)")]
        public void ToStringHandlesTokenType(bool include, string expected)
        {
            var node = new AstNode(AstNodeType.Function,
                new Token(TokenType.Identifier, "Test"),
                string.Empty);
            node.Arguments.Add(new AstNode(AstNodeType.Constant, new Token(TokenType.Number, numberValue), string.Empty));
            var opts = new AstNode.DisplayOptions
            {
                IncludeTokenTypes = include
            };
            Assert.Equal(expected, node.ToString(opts));
        }
    }
}