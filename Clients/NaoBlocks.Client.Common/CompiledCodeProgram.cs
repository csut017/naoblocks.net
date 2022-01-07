using NaoBlocks.Common;
using Newtonsoft.Json;

namespace NaoBlocks.Client.Common
{
    /// <summary>
    /// A compiled program.
    /// </summary>
    public class CompiledCodeProgram
    {
        /// <summary>
        /// Initialise a new <see cref="CompiledCodeProgram"/> instance.
        /// </summary>
        /// <param name="nodes">The initial nodes in the program.</param>
        public CompiledCodeProgram(params AstNode[] nodes)
        {
            this.Nodes = new List<AstNode>(nodes);
        }

        /// <summary>
        /// The <see cref="AstNode"/> instances containing the compiled program.
        /// </summary>
        [JsonProperty("nodes")]
        public IList<AstNode> Nodes { get; private set; } = new List<AstNode>();
    }
}
