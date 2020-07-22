using System;

namespace NaoBlocks.Core.Models
{
    public class CodeProgram
    {
        public string Code { get; set; } = string.Empty;

        public string? Name { get; set; }

        public long? Number { get; set; }

        public string UserId { get; set; } = string.Empty;

        public DateTime WhenAdded { get; set; }
    }
}