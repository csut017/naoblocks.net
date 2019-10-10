using System;
using System.Collections.Generic;
using System.Text;

namespace NaoBlocks.Parser
{
    public class ParseResult
    {
        public ICollection<ParseError> Errors { get; private set; }

        public ParseResult()
        {
            this.Errors = new List<ParseError>();
        }

        public AstNode AST { get; set; }
    }
}
