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
            foreach (var node in nodes)
            {
                var indexed = tree.IndexNode(node);
                tree.rootNodes.Add(indexed);
            }

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
                indexed.Arguments.Add(
                    this.IndexNode(argment, indexed.Index));
            }

            foreach (var child in node.Children)
            {
                indexed.Children.Add(
                    this.IndexNode(child, indexed.Index));
            }

            indexed.Lock();
            return indexed;
        }
    }
}