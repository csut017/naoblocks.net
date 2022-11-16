namespace NaoBlocks.Web.Communications
{
    /// <summary>
    /// Defines the status of a client.
    /// </summary>
    public class ClientStatus
    {
        /// <summary>
        /// Gets or sets whether the client is available.
        /// </summary>
        public bool IsAvailable { get; set; } = true;

        /// <summary>
        /// Gets or sets the last message from the client.
        /// </summary>
        public string Message { get; set; } = "Unknown";

        /// <summary>
        /// Gets or sets when the client was last allocated.
        /// </summary>
        public DateTime LastAllocatedTime { get; set; } = DateTime.MinValue;
    }
}