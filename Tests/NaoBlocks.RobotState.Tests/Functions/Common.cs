using NaoBlocks.Common;

namespace NaoBlocks.RobotState.Tests.Functions
{
    public static class Common
    {
        public static IndexedNode EmptyNode
        {
            get
            {
                return new IndexedNode(
                    new AstNode(AstNodeType.Empty, new Token(TokenType.Empty, string.Empty), string.Empty),
                    0);
            }
        }
    }
}