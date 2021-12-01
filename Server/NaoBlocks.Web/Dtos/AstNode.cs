namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// A node in an Abstract Syntax Tree.
    /// </summary>
    public class AstNode
    {
        /// <summary>
        /// Gets an empty node.
        /// </summary>
        public static AstNode Empty
        {
            get { return new AstNode(); }
        }

        /// <summary>
        /// Gets the arguments for the node.
        /// </summary>
        public IList<AstNode>? Arguments { get; private set; }

        /// <summary>
        /// Gets the children for the node.
        /// </summary>
        public IList<AstNode>? Children { get; private set; }

        /// <summary>
        /// Gets or sets the source node.
        /// </summary>
        public string SourceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the <see cref="Token"/>.
        /// </summary>
        public Token Token { get; set; } = Token.Empty;

        /// <summary>
        /// Gets or sets the node type.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Converts from a data entity to a data transfer object.
        /// </summary>
        /// <param name="value">The <see cref="Common.AstNode"/> to convert.</param>
        /// <param name="includeDetails">Whether to include the details or not.</param>
        /// <returns>A <see cref="AstNode"/> instance.</returns>
        public static AstNode? FromModel(Common.AstNode? value)
        {
            if (value == null) return null;
            var model = new AstNode
            {
                SourceId = value.SourceId,
                Token = Token.FromModel(value.Token) ?? Token.Empty,
                Type = value.Type.ToString()
            };
            if (value.Arguments != null)
            {
                model.Arguments = value.Arguments.Select(n => FromModel(n) ?? Empty).ToList();
                if (!value.Arguments.Any()) model.Arguments = null;
            }

            if (value.Children != null)
            {
                model.Children = value.Children.Select(n => FromModel(n) ?? Empty).ToList();
                if (!model.Children.Any()) model.Children = null;
            }

            return model;
        }
    }
}
