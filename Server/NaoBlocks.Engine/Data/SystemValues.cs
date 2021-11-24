namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// The system-wide values.
    /// </summary>
    public class SystemValues
    {
        /// <summary>
        /// Gets or sets the default server address.
        /// </summary>
        public string DefaultAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the next conversation identifier.
        /// </summary>
        public long NextConversationId { get; set; } = 0;
    }
}
