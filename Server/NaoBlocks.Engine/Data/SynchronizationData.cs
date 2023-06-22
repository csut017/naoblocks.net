namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// Contains the synchronization data for a record.
    /// </summary>
    public class SynchronizationData
    {
        /// <summary>
        /// Gets or sets the source this record is from.
        /// </summary>
        public string Source { get; set; } = Environment.MachineName;

        /// <summary>
        /// Gets or sets when this record was last synchronized.
        /// </summary>
        public DateTime? WhenSynced { get; set; }
    }
}