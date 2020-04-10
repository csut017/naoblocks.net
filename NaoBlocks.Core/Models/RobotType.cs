using System;

namespace NaoBlocks.Core.Models
{
    public class RobotType
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public DateTime WhenAdded { get; set; }
    }
}