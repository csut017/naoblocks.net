namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// Defines a source that this machine can synchronize with.
    /// </summary>
    public class SynchronizationSource
    {
        /// <summary>
        /// Gets or sets the address of the machine.
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Gets or sets a human-friendly name for the machine.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the authentication token.
        /// </summary>
        public string? Token { get; set; }

        /// <summary>
        /// Gets or sets when this machine was last synchronized.
        /// </summary>
        public DateTime? WhenLastSynced { get; set; }
    }
}