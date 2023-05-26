namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// Contains the synchronization status for a subsystem.
    /// </summary>
    public class SynchronizationStatus
    {
        /// <summary>
        /// Gets or sets the name of the subsystem.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets when the subsystem was last updated.
        /// </summary>
        public DateTime? WhenLastUpdated { get; set; }
    }
}
