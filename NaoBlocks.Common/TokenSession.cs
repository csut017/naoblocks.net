using System;

namespace NaoBlocks.Common
{
    public class TokenSession
    {
        public DateTime Expires { get; set; } = DateTime.MinValue;

        public string Role { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;
    }
}
