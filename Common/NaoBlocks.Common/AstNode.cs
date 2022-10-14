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
        /// Defines the type of display.
        /// </summary>
        public enum DisplayType
        {
            /// <summary>
            /// Includes the items.
            /// </summary>
            Include,

            /// <summary>
            /// Excludes the items, but contains the wrapping elements.
            /// </summary>
            Exclude,

            /// <summary>
            /// Ignores the items, including the wrapping elements.
            /// </summary>
            Ignore,
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

            if (options.IncludeNodeTypes)
            {
                output.Append($"{Type}:");
            }

            output.Append(Token.Value);
            if (options.IncludeTokenTypes)
            {
                output.Append($"=>{Token.Type.ToString().ToUpperInvariant()}");
            }

            if (this.Arguments.Any() == true)
            {
                switch (options.Arguments)
                {
                    case DisplayType.Exclude:
                        output.Append("()");
                        break;

                    case DisplayType.Include:
                        output.Append("(" + string.Join(",", this.Arguments.Select(arg => arg.ToString(options))) + ")");
                        break;
                }
            }

            if (this.Children.Any() == true)
            {
                switch (options.Children)
                {
                    case DisplayType.Exclude:
                        output.Append("{}");
                        break;

                    case DisplayType.Include:
                        output.Append("{" + string.Join(",", this.Children.Select(arg => arg.ToString(options))) + "}");
                        break;
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
            /// Initialises a new <see cref="DisplayOptions"/> instance.
            /// </summary>
            public DisplayOptions()
            {
                this.Arguments = DisplayType.Include;
                this.Children = DisplayType.Include;
            }

            /// <summary>
            /// How arguments are displayed.
            /// </summary>
            public DisplayType Arguments { get; set; }

            /// <summary>
            /// How children are displayed.
            /// </summary>
            public DisplayType Children { get; set; }

            /// <summary>
            /// Include the node type.
            /// </summary>
            public bool IncludeNodeTypes { get; set; } = false;

            /// <summary>
            /// Include source IDs.
            /// </summary>
            public bool IncludeSourceIDs { get; set; } = false;

            /// <summary>
            /// Include the token type.
            /// </summary>
            public bool IncludeTokenTypes { get; set; } = false;
        }
    }
}