namespace NaoBlocks.Web.Dtos
{
    public class RobotStatus
    {
        public string FriendlyName { get; set; } = string.Empty;

        public long Id { get; set; }

        public bool IsAvailable { get; set; }

        public string MachineName { get; set; } = string.Empty;

        public string Status { get; set; } = "Unknown";
    }
}