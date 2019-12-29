using System;

namespace NaoBlocks.Core.Models
{
    public class CodeProgram
    {
        public string Code { get; set; } = string.Empty;

        public string? Id { get; set; }

        public string? Name { get; set; }

        public DateTime WhenAdded { get; set; }
    }
}