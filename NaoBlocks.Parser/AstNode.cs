using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NaoBlocks.Parser
{
    public class AstNode
    {
        public AstNode(AstNodeType type, Token token)
        {
            this.Type = type;
            this.Token = token;
            this.Arguments = new List<AstNode>();
            this.Children = new List<AstNode>();
        }

        public Token Token { get; set; }

        public AstNodeType Type { get; set; }

        public string SourceId { get; set; }

        public IList<AstNode> Arguments { get; private set; }

        public IList<AstNode> Children { get; private set; }

        public override string ToString()
        {
            var output = this.Type.ToString() + ":" + this.Token.Value;
            if (this.Arguments.Any())
            {
                output += "("
                    + string.Join(",", this.Arguments.Select(arg => arg.ToString()))
                    + ")";
            }

            if (this.Children.Any())
            {
                output += "{"
                    + string.Join(",", this.Children.Select(arg => arg.ToString()))
                    + "}";
            }

            return output;
        }
    }
}
