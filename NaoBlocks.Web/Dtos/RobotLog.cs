using System;
using System.Collections.Generic;
using System.Linq;
using Data = NaoBlocks.Core.Models;

namespace NaoBlocks.Web.Dtos
{
    public class RobotLog
    {
        public long ConversationId { get; set; }

        public string? UserName { get; set; }

        public IList<RobotLogLine>? Lines { get; private set; }

        public DateTime WhenAdded { get; set; } = DateTime.UtcNow;

        public DateTime WhenLastUpdated { get; set; } = DateTime.UtcNow;

        public static RobotLog FromModel(Data.RobotLog value, bool includeLines, Data.Conversation? converstion)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            var robotLog = new RobotLog
            {
                ConversationId = value.ConversationId,
                WhenAdded = value.WhenAdded,
                WhenLastUpdated = value.WhenLastUpdated
            };
            if (converstion != null) robotLog.UserName = converstion.UserName;
            if (includeLines)
            {
                robotLog.Lines = value.Lines.Select(RobotLogLine.FromModel).ToList();
            }

            return robotLog;
        }
    }
}