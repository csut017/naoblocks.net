using System;

namespace NaoBlocks.Web.Dtos
{
    public class Session
    {
        public DateTime Expires { get; set; } = DateTime.MinValue;

        public string Role { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;
    }
}