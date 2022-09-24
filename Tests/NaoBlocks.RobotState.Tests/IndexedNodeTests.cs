using NaoBlocks.Common;

namespace NaoBlocks.RobotState.Tests
{
    public class IndexedNodeTests
    {
        [Fact]
        public void CanSetNext()
        {
            var ast = new AstNode(AstNodeType.Empty, Token.Empty, string.Empty);
            var node = new IndexedNode(ast, 3)
            {
                Next = 5
            };
            Assert.Equal(5, node.Next);
        }

        [Fact]
        public void ConstructorSetsProperties()
        {
            var ast = new AstNode(AstNodeType.Empty, Token.Empty, string.Empty);
            var node = new IndexedNode(ast, 1, 2);
            Assert.Same(ast, node.Node);
            Assert.Equal(1, node.Index);
            Assert.Equal(2, node.Parent);
        }

        [Fact]
        public void LockLocksNext()
        {
            var ast = new AstNode(AstNodeType.Empty, Token.Empty, string.Empty);
            var node = new IndexedNode(ast, 3);
            node.Lock();
            var ex = Assert.Throws<InvalidOperationException>(() => node.Next = 5);
            Assert.Equal(-1, node.Next);
            Assert.Equal(
                "Node has been locked, cannot modify",
                ex.Message);
        }

        [Fact]
        public void ToStringHandlesArguments()
        {
            var ast = new AstNode(
                AstNodeType.Function,
                new Token(TokenType.Identifier, "Test"),
                string.Empty);
            ast.Arguments.Add(new AstNode(
                AstNodeType.Constant,
                new Token(TokenType.Number, "123"),
                string.Empty));
            var node = AstProgram.New(new[] { ast }).RootNodes[0];
            Assert.Equal(
                "0=>Function:Test(1=>Constant:123)",
                node.ToString());
        }

        [Fact]
        public void ToStringHandlesArgumentsAndChildren()
        {
            var ast = new AstNode(
                AstNodeType.Function,
                new Token(TokenType.Identifier, "Test"),
                string.Empty);
            ast.Arguments.Add(new AstNode(
                AstNodeType.Constant,
                new Token(TokenType.Number, "123"),
                string.Empty));
            ast.Children.Add(new AstNode(
                AstNodeType.Function,
                new Token(TokenType.Identifier, "Inner"),
                string.Empty));
            var node = AstProgram.New(new[] { ast }).RootNodes[0];
            Assert.Equal(
                "0=>Function:Test(1=>Constant:123){2=>Function:Inner}",
                node.ToString());
        }

        [Fact]
        public void ToStringHandlesChildren()
        {
            var ast = new AstNode(
                AstNodeType.Function,
                new Token(TokenType.Identifier, "Test"),
                string.Empty);
            ast.Children.Add(new AstNode(
                AstNodeType.Function,
                new Token(TokenType.Identifier, "Inner"),
                string.Empty));
            var node = AstProgram.New(new[] { ast }).RootNodes[0];
            Assert.Equal(
                "0=>Function:Test{1=>Function:Inner}",
                node.ToString());
        }

        [Fact]
        public void ToStringHandlesIndicating()
        {
            var ast = new AstNode(AstNodeType.Empty, Token.Empty, string.Empty);
            var node = AstProgram.New(new[] { ast }).RootNodes[0];
            Assert.Equal(
                "0**=>Empty:",
                node.ToString(0));
        }

        [Fact]
        public void ToStringReturnsInnerNodeAndIndex()
        {
            var ast = new AstNode(AstNodeType.Empty, Token.Empty, string.Empty);
            var node = AstProgram.New(new[] { ast }).RootNodes[0];
            Assert.Equal(
                "0=>Empty:",
                node.ToString());
        }
    }
}