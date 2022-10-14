using NaoBlocks.Common;

namespace NaoBlocks.Parser
{
    /// <summary>
    /// The results of parsing a NaoLang program.
    /// </summary>
    public class ParseResult
    {
        /// <summary>
        /// Initialises a new <see cref="ParseResult"/>.
        /// </summary>
        public ParseResult()
        {
            this.Errors = new List<ParseError>();
            this.Nodes = new List<AstNode>();
        }

        /// <summary>
        /// The errors from the parse operation.
        /// </summary>
        public IList<ParseError> Errors { get; private set; }

        /// <summary>
        /// The generated Abstract Syntax Tree.
        /// </summary>
        public IList<AstNode> Nodes { get; private set; }
    }
}