namespace NaoBlocks.Web.Dtos
{
    public class UserStatus
    {
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Status { get; set; } = "Unknown";
    }
}