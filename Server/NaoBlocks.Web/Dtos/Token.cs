namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// A parsed code token.
    /// </summary>
    public class Token
    {
        /// <summary>
        /// Gets an empty token.
        /// </summary>
        public static Token Empty
        {
            get { return new Token(); }
        }

        /// <summary>
        /// Gets or sets the line number.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Gets or sets the position within the line.
        /// </summary>
        public int LinePosition { get; set; }

        /// <summary>
        /// Gets or sets the type of the token.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value of the token.
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Converts from a data entity to a data transfer object.
        /// </summary>
        /// <param name="value">The <see cref="Common.Token"/> to convert.</param>
        /// <returns>A <see cref="Token"/> instance.</returns>
        public static Token? FromModel(Common.Token? value)
        {
            return value == null ? null : new Token
            {
                LineNumber = value.LineNumber,
                LinePosition = value.LinePosition,
                Type = value.Type.ToString(),
                Value = value.Value
            };
        }
    }
}