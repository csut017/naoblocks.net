using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NaoBlocks.Parser.Tests
{
    internal class TokenComparer : IEqualityComparer<Token>
    {
        public bool Equals([AllowNull] Token x, [AllowNull] Token y)
        {
            return (x.Type == y.Type)
                && (x.Value == y.Value);
        }

        public int GetHashCode([DisallowNull] Token token)
        {
            return token.GetHashCode();
        }
    }
}