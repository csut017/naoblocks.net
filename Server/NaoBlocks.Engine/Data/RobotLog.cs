namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// Stores an execution log for when a program was run.
    /// </summary>
    public class RobotLog
    {
        /// <summary>
        /// Gets or sets the conversation this log is part of.
        /// </summary>
        public Conversation Conversation { get; set; } = Conversation.None;

        /// <summary>
        /// Gets or sets the id of the log.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets the lines in the log.
        /// </summary>
        public IList<RobotLogLine> Lines { get; } = new List<RobotLogLine>();

        /// <summary>
        /// Gets or sets the assigned robot id.
        /// </summary>
        public string RobotId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets when this log was added.
        /// </summary>
        public DateTime WhenAdded { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets when this log was last added to.
        /// </summary>
        public DateTime WhenLastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the details about when this log was last synchronized.
        /// </summary>
        public SynchronizationStatus Synchronization { get; set; } = new SynchronizationStatus();
    }
}