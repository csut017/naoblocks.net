using NaoBlocks.Common;

namespace NaoBlocks.RobotState
{
    /// <summary>
    /// Represents the Abstract Syntax Tree in memory.
    /// </summary>
    public class AstProgram
    {
        private readonly List<IndexedNode> nodeSequence = new();
        private readonly List<IndexedNode> rootNodes = new();
        private int currentIndex = -1;

        private AstProgram()
        {
        }

        /// <summary>
        /// Gets the total number of nodes in the program.
        /// </summary>
        public int Count
        {
            get { return this.currentIndex + 1; }
        }

        /// <summary>
        /// Gets the root nodes for the program.
        /// </summary>
        public IReadOnlyList<IndexedNode> RootNodes
        {
            get { return this.rootNodes.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="AstNode"/> at the specified index.
        /// </summary>
        /// <param name="index">The index of the node.</param>
        /// <returns>The <see cref="AstNode"/> instance.</returns>
        public IndexedNode this[int index]
        {
            get { return this.nodeSequence[index]; }
        }

        /// <summary>
        /// Initialises a new <see cref="AstProgram"/> from a set of <see cref="AstNode"/> instances.
        /// </summary>
        /// <param name="nodes">The nodes to populate the program.</param>
        /// <returns>The new <see cref="AstProgram"/> instance.</returns>
        public static AstProgram New(IEnumerable<AstNode> nodes)
        {
            var tree = new AstProgram();
            IndexedNode? lastNode = null;
            foreach (var node in nodes)
            {
                var indexed = tree.IndexNode(node);
                if (lastNode != null)
                {
                    lastNode.Next = indexed.Index;
                    lastNode.Lock();
                }
                lastNode = indexed;
                tree.rootNodes.Add(indexed);
            }

            if (lastNode != null) lastNode.Lock();
            return tree;
        }

        /// <summary>
        /// Generates a string representation of this program.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return this.ToString(-1);
        }

        /// <summary>
        /// Generates a string representation of this program.
        /// </summary>
        /// <param name="nodeToIndicate">A node to indicate.</param>
        /// <returns>The string representation.</returns>
        public string ToString(int nodeToIndicate)
        {
            return string.Join(
                Environment.NewLine,
                this.rootNodes.Select(n => n.ToString(nodeToIndicate)));
        }

        private IndexedNode IndexNode(AstNode node, int? parent = null)
        {
            var indexed = new IndexedNode(node, ++this.currentIndex, parent);
            this.nodeSequence.Add(indexed);
            foreach (var argment in node.Arguments)
            {
                var argNode = this.IndexNode(argment, indexed.Index);
                indexed.Arguments.Add(argNode);
                argNode.Lock();
            }

            IndexedNode? lastNode = null;
            foreach (var child in node.Children)
            {
                var childNode = this.IndexNode(child, indexed.Index);
                indexed.Children.Add(childNode);
                if (lastNode != null)
                {
                    lastNode.Next = childNode.Index;
                    lastNode.Lock();
                }
                lastNode = childNode;
                if (lastNode != null) lastNode.Lock();
            }

            return indexed;
        }
    }
}