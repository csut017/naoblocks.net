namespace NaoBlocks.Common
{
    /// <summary>
    /// The basic building block of a compiled NaoLang program.
    /// </summary>
    /// <remarks>
    /// A NaoLang program is compiled in two steps:
    /// 1. Generates a sequence of <see cref="Token"/> elements,
    /// 2. Which are then combined into an Abstract Syntax Tree.
    /// </remarks>
    public class Token
    {
        /// <summary>
        /// Initialise a new empty <see cref="Token"/> instance.
        /// </summary>
        public Token()
            : this(TokenType.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Initialises a new populated <see cref="Token"/> instance.
        /// </summary>
        /// <param name="type">The type of the token.</param>
        /// <param name="value">The value of the token.</param>
        public Token(TokenType type, string value)
        {
            this.Type = type;
            this.Value = value;
        }

        /// <summary>
        /// Initialises a new populated <see cref="Token"/> instance with line details.
        /// </summary>
        /// <param name="type">The type of the token.</param>
        /// <param name="value">The value of the token.</param>
        /// <param name="lineNumber">The line number where the token is from.</param>
        /// <param name="linePosition">The position within the line.</param>
        public Token(TokenType type, string value, int lineNumber, int linePosition)
            : this(type, value)
        {
            this.LineNumber = lineNumber;
            this.LinePosition = linePosition;
        }

        /// <summary>
        /// An empty <see cref="Token"/> instance.
        /// </summary>
        public static Token Empty 
        { 
            get { return new Token(TokenType.Empty, string.Empty); }
        }

        /// <summary>
        /// The type of token.
        /// </summary>
        public TokenType Type { get; set; }

        /// <summary>
        /// The value of the token.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The line number where the token is from.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// The position in the line where the token is from.
        /// </summary>
        public int LinePosition { get; set; }

        /// <summary>
        /// Generates a human-readable version of this <see cref="Token"/>.
        /// </summary>
        /// <returns>A string containing the human-readable format.</returns>
        public override string ToString()
        {
            return $"{{{Value}:{Type}}}";
        }
    }
}
