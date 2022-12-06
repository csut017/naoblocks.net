using NaoBlocks.Common;

namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// Defines a template for direct logging robots.
    /// </summary>
    public class LoggingTemplate
    {
        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the message type.
        /// </summary>
        public ClientMessageType MessageType { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the names of the values.
        /// </summary>
        public string[] ValueNames { get; set; } = Array.Empty<string>();
    }
}