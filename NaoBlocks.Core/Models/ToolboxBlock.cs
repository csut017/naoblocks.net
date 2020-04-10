using System;
using System.Collections.Generic;
using System.Text;

namespace NaoBlocks.Core.Models
{
    public class ToolboxBlock
    {
        public string Name { get; set; } = string.Empty;

        public string Definition { get; set; } = string.Empty;

        public int Order { get; set; }
    }
}
