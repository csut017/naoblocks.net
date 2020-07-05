using System;

namespace NaoBlocks.Web.Communications
{
    public class ClientStatus
    {
        public bool IsAvailable { get; set; } = true;

        public string Message { get; set; } = "Unknown";

        public DateTime LastAllocatedTime { get; set; } = DateTime.MinValue;
    }
}