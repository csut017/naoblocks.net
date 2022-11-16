using NaoBlocks.Common;
using System.Collections.Generic;

namespace NaoBlocks.Parser.Tests
{
    internal class TokenComparer : IEqualityComparer<Token>
    {
        public bool Equals(Token? x, Token? y)
        {
            return (x?.Type == y?.Type)
                && (x?.Value == y?.Value);
        }

        public int GetHashCode(Token token)
        {
            return token.GetHashCode();
        }
    }
}