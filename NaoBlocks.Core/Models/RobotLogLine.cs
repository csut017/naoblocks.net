using System;
using System.Collections.Generic;

namespace NaoBlocks.Core.Models
{
    public class RobotLogLine
    {
        public string Description { get; set; } = string.Empty;

        public ClientMessageType SourceMessageType { get; set; }

        public IList<RobotLogLineValue> Values { get; } = new List<RobotLogLineValue>();

        public DateTime WhenAdded { get; set; } = DateTime.UtcNow;
    }
}