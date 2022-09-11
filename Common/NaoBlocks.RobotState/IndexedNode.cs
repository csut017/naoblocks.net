using NaoBlocks.Common;

namespace NaoBlocks.RobotState
{
    /// <summary>
    /// Contains an <see cref="AstNode"/> that has been indexed.
    /// </summary>
    public class IndexedNode
    {
        private IList<IndexedNode> arguments = new List<IndexedNode>();
        private IList<IndexedNode> children = new List<IndexedNode>();

        /// <summary>
        /// Initialises a new <see cref="IndexedNode"/> instance.
        /// </summary>
        /// <param name="node">The node to index.</param>
        /// <param name="index">The index number.</param>
        /// <param name="parent">The index of the parent node.</param>
        public IndexedNode(AstNode node, int index, int? parent = null)
        {
            this.Node = node;
            this.Index = index;
            this.Parent = parent;
        }

        /// <summary>
        /// Any arguments that were passed into the node.
        /// </summary>
        public IList<IndexedNode> Arguments
        {
            get { return this.arguments; }
        }

        /// <summary>
        /// Any children for the node.
        /// </summary>
        public IList<IndexedNode> Children
        {
            get { return this.children; }
        }

        /// <summary>
        /// The node's index number.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets the node.
        /// </summary>
        public AstNode Node { get; }

        /// <summary>
        /// The index number of the node's parent.
        /// </summary>
        public int? Parent { get; }

        /// <summary>
        /// Locks this node so it can't be modified.
        /// </summary>
        public void Lock()
        {
            this.arguments = ((List<IndexedNode>)this.arguments).AsReadOnly();
            this.children = ((List<IndexedNode>)this.children).AsReadOnly();
        }
    }
}