using System;
using System.Collections.Generic;
using System.Text;

namespace NaoBlocks.Parser
{
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
        Colour
    }
}
