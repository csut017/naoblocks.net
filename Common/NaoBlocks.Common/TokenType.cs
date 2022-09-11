namespace NaoBlocks.Common
{
    /// <summary>
    /// Defines the type of token.
    /// </summary>
    /// <remarks>
    /// A <see cref="Token"/> is the basic building block of a NaoLang program.
    /// This enumeration defines the token type, which will influence how the
    /// block is executed.
    /// </remarks>
    public enum TokenType
    {
        /// <summary>
        /// The token is illegal (not understood by the scanner.)
        /// </summary>
        Illegal,

        /// <summary>
        /// End of File.
        /// </summary>
        EOF,

        /// <summary>
        /// Whitespace.
        /// </summary>
        Whitespace,

        /// <summary>
        /// An identifier.
        /// </summary>
        Identifier,

        /// <summary>
        /// A number ([0-9]+[\.]?[0-9]*)
        /// </summary>
        Number,

        /// <summary>
        /// New line (\r, \n, or any combination thereof).
        /// </summary>
        Newline,

        /// <summary>
        /// A opening brace.
        /// </summary>
        OpenBrace,

        /// <summary>
        /// A closing brace.
        /// </summary>
        CloseBrace,

        /// <summary>
        /// A opening bracket.
        /// </summary>
        OpenBracket,

        /// <summary>
        /// A closing bracket.
        /// </summary>
        CloseBracket,

        /// <summary>
        /// A comma.
        /// </summary>
        Comma,

        /// <summary>
        /// Text (starts and ends with ').
        /// </summary>
        Text,

        /// <summary>
        /// A constant ([A-Z]+).
        /// </summary>
        Constant,

        /// <summary>
        /// A source identifier.
        /// </summary>
        SourceID,

        /// <summary>
        /// A variable (starts with @).
        /// </summary>
        Variable,

        /// <summary>
        /// A boolean (TRUE or FALSE).
        /// </summary>
        Boolean,

        /// <summary>
        /// A generated value.
        /// </summary>
        Generated,

        /// <summary>
        /// A colour (starts with #).
        /// </summary>
        Colour,

        /// <summary>
        /// An empty token.
        /// </summary>
        Empty
    }
}