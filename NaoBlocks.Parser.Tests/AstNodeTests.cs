using System.Linq;
using Xunit;

namespace NaoBlocks.Parser.Tests
{
    public class AstNodeTests
    {
        [Fact]
        public void CleanLeavesExistingArguments()
        {
            var node = new AstNode(AstNodeType.Function, new Token(TokenType.Boolean, "TRUE"), string.Empty);
            node.Arguments.Add(new AstNode(AstNodeType.Argument, new Token(TokenType.Identifier, "Test"), string.Empty));
            var cleaned = node.Clean();
            Assert.NotNull(cleaned.Arguments);
            Assert.Equal(node.Arguments.Select(n => n.ToString()), cleaned.Arguments.Select(n => n.ToString()));
        }

        [Fact]
        public void CleanLeavesExistingChildren()
        {
            var node = new AstNode(AstNodeType.Function, new Token(TokenType.Boolean, "TRUE"), string.Empty);
            node.Children.Add(new AstNode(AstNodeType.Argument, new Token(TokenType.Identifier, "Test"), string.Empty));
            var cleaned = node.Clean();
            Assert.NotNull(cleaned.Children);
            Assert.Equal(node.Children.Select(n => n.ToString()), cleaned.Children.Select(n => n.ToString()));
        }

        [Fact]
        public void CleanRemovesEmptyArguments()
        {
            var node = new AstNode(AstNodeType.Function, new Token(TokenType.Boolean, "TRUE"), string.Empty);
            var cleaned = node.Clean();
            Assert.Null(cleaned.Arguments);
        }

        [Fact]
        public void CleanRemovesEmptyChildren()
        {
            var node = new AstNode(AstNodeType.Function, new Token(TokenType.Boolean, "TRUE"), string.Empty);
            var cleaned = node.Clean();
            Assert.Null(cleaned.Children);
        }
    }
}