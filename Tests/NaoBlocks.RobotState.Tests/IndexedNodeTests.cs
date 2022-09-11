using NaoBlocks.Common;

namespace NaoBlocks.RobotState.Tests
{
    public class IndexedNodeTests
    {
        [Fact]
        public void ConstructorSetsProperties()
        {
            var ast = new AstNode(AstNodeType.Empty, Token.Empty, string.Empty);
            var node = new IndexedNode(ast, 1, 2);
            Assert.Same(ast, node.Node);
            Assert.Equal(1, node.Index);
            Assert.Equal(2, node.Parent);
        }
    }
}