using NaoBlocks.Common;
using System.Collections.ObjectModel;

namespace NaoBlocks.RobotState
{
    /// <summary>
    /// Represents the Abstract Syntax Tree in memory.
    /// </summary>
    public class AstProgram
    {
        private readonly List<IndexedNode> nodeSequence = new List<IndexedNode>();
        private readonly List<IndexedNode> rootNodes = new List<IndexedNode>();
        private readonly IReadOnlyList<IndexedNode> rootNodesAsReadonly;
        private int currentIndex = -1;

        private AstProgram()
        {
            this.rootNodesAsReadonly = new ReadOnlyCollection<IndexedNode>(this.rootNodes);
        }

        /// <summary>
        /// Gets the root nodes for the program.
        /// </summary>
        public IReadOnlyList<IndexedNode> RootNodes
        {
            get { return this.rootNodesAsReadonly; }
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

        private IndexedNode IndexNode(AstNode node, int? parent = null)
        {
            var indexed = new IndexedNode(node, ++this.currentIndex, parent);
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