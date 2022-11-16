using NaoBlocks.Common;
using NaoBlocks.Parser;

namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// A compiled code program.
    /// </summary>
    public class CompiledCodeProgram
    {
        /// <summary>
        /// Initialise a new <see cref="CompiledCodeProgram"/> instance from a <see cref="ParseResult"/>.
        /// </summary>
        /// <param name="result">The <see cref="ParseResult"/> instance.</param>
        public CompiledCodeProgram(ParseResult result)
        {
            if (result.Errors.Any()) this.Errors = result.Errors;
            if (result.Nodes.Any()) this.Nodes = result.Nodes;
        }

        /// <summary>
        /// Gets or sets the errors.
        /// </summary>
        public IEnumerable<ParseError>? Errors { get; private set; }

        /// <summary>
        /// Gets or sets the nodes.
        /// </summary>
        public IEnumerable<AstNode>? Nodes { get; private set; }

        /// <summary>
        /// Gets or sets the program id.
        /// </summary>
        public long? ProgramId { get; set; }
    }
}
