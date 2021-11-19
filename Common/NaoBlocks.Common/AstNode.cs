using System.Text;

namespace NaoBlocks.Common
{
    /// <summary>
    /// A node in an Abstract Syntax Tree.
    /// </summary>
    public class AstNode
    {
        /// <summary>
        /// Initialise a pre-populated <see cref="AstNode"/>.
        /// </summary>
        /// <param name="type">The type of the node.</param>
        /// <param name="token">The source token for the node.</param>
        /// <param name="sourceId">The original identifier.</param>
        public AstNode(AstNodeType type, Token token, string sourceId)
        {
            this.Type = type;
            this.Token = token;
            this.SourceId = sourceId;
            this.Arguments = new List<AstNode>();
            this.Children = new List<AstNode>();
        }

        /// <summary>
        /// Any arguments that were passed into the node.
        /// </summary>
        public IList<AstNode> Arguments { get; private set; }

        /// <summary>
        /// Any children for the node.
        /// </summary>
        public IList<AstNode> Children { get; private set; }

        /// <summary>
        /// A source ID for use in displaying debug statements in an execution environment.
        /// </summary>
        public string SourceId { get; set; }

        /// <summary>
        /// The original <see cref="Token"/> that the node was generated from.
        /// </summary>
        public Token Token { get; set; }

        /// <summary>
        /// The type of the node.
        /// </summary>
        public AstNodeType Type { get; set; }

        /// <summary>
        /// Generates a human-readable version of this node using the default options.
        /// </summary>
        /// <param name="options">The options for generating the string.</param>
        /// <returns>A string containing the human-readable version.</returns>
        public string ToString(DisplayOptions options)
        {
            var output = new StringBuilder();
            if (options.IncludeSourceIDs && !string.IsNullOrEmpty(this.SourceId))
            {
                output.Append($"[{SourceId}]");
            }

            output.Append($"{Type}:{Token.Value}");
            if (options.IncludeTokenTypes)
            {
                output.Append($"=>{Token.Type.ToString().ToUpperInvariant()}");
            }

            if (this.Arguments.Any() == true)
            {
                if (options.ExcludeArguments)
                {
                    output.Append("()");
                }
                else
                {
                    output.Append("(" + string.Join(",", this.Arguments.Select(arg => arg.ToString(options))) + ")");
                }
            }

            if (this.Children.Any() == true)
            {
                if (options.ExcludeChildren)
                {
                    output.Append("{}");
                }
                else
                {
                    output.Append("{" + string.Join(",", this.Children.Select(arg => arg.ToString(options))) + "}");
                }
            }

            return output.ToString();
        }

        /// <summary>
        /// Generates a human-readable version of this node using the default options.
        /// </summary>
        /// <returns>A string containing the human-readable version.</returns>
        public override string ToString()
        {
            return this.ToString(new DisplayOptions
            {
                IncludeSourceIDs = false,
                IncludeTokenTypes = false                
            });
        }

        /// <summary>
        /// The display options for generating the human-readable version.
        /// </summary>
        public struct DisplayOptions
        {
            /// <summary>
            /// Include source IDs.
            /// </summary>
            public bool IncludeSourceIDs { get; set; }

            /// <summary>
            /// Include the token type.
            /// </summary>
            public bool IncludeTokenTypes { get; set; }

            /// <summary>
            /// Exxclude arguments.
            /// </summary>
            public bool ExcludeArguments { get; set; }

            /// <summary>
            /// Exclude children.
            /// </summary>
            public bool ExcludeChildren { get; set; }
        }
    }
}