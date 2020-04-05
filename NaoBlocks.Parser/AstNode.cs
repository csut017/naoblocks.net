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

        public IList<AstNode> Arguments { get; private set; }

        public IList<AstNode> Children { get; private set; }

        public string SourceId { get; set; }

        public Token Token { get; set; }

        public AstNodeType Type { get; set; }

        public AstNode Clean()
        {
            var newNode = new AstNode(this.Type, this.Token, this.SourceId)
            {
                Arguments = this.Arguments.Any()
                    ? this.Arguments.Select(n => n.Clean()).ToList()
                    : null,
                Children = this.Children.Any()
                    ? this.Children.Select(n => n.Clean()).ToList()
                    : null
            };
            return newNode;
        }

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

            if (this.Arguments?.Any() == true)
            {
                output.Append("(" + string.Join(",", this.Arguments.Select(arg => arg.ToString(options))) + ")");
            }

            if (this.Children?.Any() == true)
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