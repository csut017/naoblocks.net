using NaoBlocks.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Dtos
{
    public class RobotCodeCompilation
    {
        public RobotCodeCompilation(ParseResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            this.Errors = result.Errors;
            this.Nodes = result.Nodes;
        }

        public IEnumerable<ParseError> Errors { get; private set; }

        public IEnumerable<AstNode> Nodes { get; private set; }
    }
}
