using System;

namespace NaoBlocks.Web.Communications
{
    public class NotificationAlert
    {
        public int Id { get; set; } = -1;

        public string Message { get; set; } = string.Empty;

        public string Severity { get; set; } = "info";

        public DateTime WhenAdded { get; set; } = DateTime.MinValue;
    }
}