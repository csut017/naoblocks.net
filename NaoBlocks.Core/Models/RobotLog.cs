using System;
using System.Collections.Generic;

namespace NaoBlocks.Core.Models
{
    public class RobotLog
    {
        public Conversation Conversation { get; set; }

        public string? Id { get; set; }

        public IList<RobotLogLine> Lines { get; } = new List<RobotLogLine>();

        public string RobotId { get; set; } = string.Empty;

        public DateTime WhenAdded { get; set; } = DateTime.UtcNow;

        public DateTime WhenLastUpdated { get; set; } = DateTime.UtcNow;
    }
}