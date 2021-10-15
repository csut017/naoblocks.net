using NaoBlocks.Common;
using System.Collections.Generic;

namespace NaoBlocks.Parser
{
    public class ParseResult
    {
        public ParseResult()
        {
            this.Errors = new List<ParseError>();
            this.Nodes = new List<AstNode>();
        }

        public ICollection<ParseError> Errors { get; private set; }

        public ICollection<AstNode> Nodes { get; private set; }
    }
}
