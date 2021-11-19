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
        Illegal,
        EOF,
        Whitespace,
        Identifier,
        Number,
        Newline,
        OpenBrace,
        CloseBrace,
        OpenBracket,
        CloseBracket,
        Comma,
        Text,
        Constant,
        SourceID,
        Variable,
        Boolean,
        Generated,
        Colour,
        Empty
    }
}