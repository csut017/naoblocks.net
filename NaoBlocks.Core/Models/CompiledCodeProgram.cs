using NaoBlocks.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NaoBlocks.Core.Models
{
    public class CompiledCodeProgram
    {
        public CompiledCodeProgram(ParseResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            if (result.Errors.Any()) this.Errors = result.Errors;
            if (result.Nodes.Any())
            {
                this.Nodes = result.Nodes.Select(node => node.Clean());
            }
        }

        public IEnumerable<ParseError>? Errors { get; private set; }

        public IEnumerable<AstNode>? Nodes { get; private set; }

        public long? ProgramId { get; set; }
    }
}