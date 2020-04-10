using System;
using System.Collections.Generic;
using System.Text;

namespace NaoBlocks.Core.Models
{
    public class ToolboxCategory
    {
        public string Name { get; set; } = string.Empty;

        public string Colour { get; set; } = "0";

        public string? Custom { get; set; }

        public int Order { get; set; }

        public IList<string> Tags { get; private set; } = new List<string>();

        public IList<ToolboxBlock> Blocks { get; private set; } = new List<ToolboxBlock>();
    }
}
