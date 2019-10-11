using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NaoBlocks.Parser
{
    public class AstNode
    {
        public AstNode(AstNodeType type, Token token, string sourceId)
        {
            this.Type = type;
            this.Token = token;
            this.SourceId = sourceId;
            this.Arguments = new List<AstNode>();
            this.Children = new List<AstNode>();
        }

        public Token Token { get; set; }

        public AstNodeType Type { get; set; }

        public string SourceId { get; set; }

        public IList<AstNode> Arguments { get; private set; }

        public IList<AstNode> Children { get; private set; }

        public string ToString(DisplayOptions options)
        {
            var output = new StringBuilder();
            if (options.IncludeSourceIDs && !string.IsNullOrEmpty(this.SourceId))
            {
                output.Append("[[" + this.SourceId + "]");
            }

            output.Append(this.Type.ToString() + ":" + this.Token.Value);
            if (options.IncludeTokenTypes)
            {
                output.Append("[:" + this.Token.Type.ToString().ToUpperInvariant() + "]");
            }

            if (this.Arguments.Any())
            {
                output.Append("(" + string.Join(",", this.Arguments.Select(arg => arg.ToString(options))) + ")");
            }

            if (this.Children.Any())
            {
                output.Append("{" + string.Join(",", this.Children.Select(arg => arg.ToString(options))) + "}");
            }

            return output.ToString();
        }

        public override string ToString()
        {
            return this.ToString(new DisplayOptions
                {
                    IncludeSourceIDs = false,
                    IncludeTokenTypes = false
                });
        }

        public struct DisplayOptions
        {
            public bool IncludeSourceIDs { get; set; }

            public bool IncludeTokenTypes { get; set; }
        }
    }
}
