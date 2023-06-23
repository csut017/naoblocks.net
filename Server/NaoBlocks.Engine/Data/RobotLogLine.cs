using NaoBlocks.Common;

namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// Defines an event (line) in a robot log.
    /// </summary>
    public class RobotLogLine
    {
        /// <summary>
        /// Gets or sets the message type on the client.
        /// </summary>
        public string? ClientMessageType { get; set; }

        /// <summary>
        /// Gets or sets the description of the event.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the source message.
        /// </summary>
        public ClientMessageType SourceMessageType { get; set; }

        /// <summary>
        /// Gets or sets the values for the event.
        /// </summary>
        public IList<NamedValue> Values { get; } = new List<NamedValue>();

        /// <summary>
        /// Gets or sets when the event occurred.
        /// </summary>
        public DateTime WhenAdded { get; set; } = DateTime.UtcNow;
    }
}