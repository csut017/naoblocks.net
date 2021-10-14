using NaoBlocks.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Data = NaoBlocks.Core.Models;

namespace NaoBlocks.Web.Dtos
{
    public class RobotLogLine
    {
        public string Description { get; set; } = string.Empty;

        public ClientMessageType SourceMessageType { get; set; }

        public IDictionary<string, string>? Values { get; private set; }

        public DateTime WhenAdded { get; set; } = DateTime.UtcNow;

        public static RobotLogLine FromModel(Data.RobotLogLine value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            var robotLog = new RobotLogLine
            {
                Description = value.Description,
                WhenAdded = value.WhenAdded,
                SourceMessageType = value.SourceMessageType
            };
            if (value.Values.Any())
            {
                robotLog.Values = value.Values.ToDictionary(v => v.Name, v => v.Value);
            }

            return robotLog;
        }
    }
}