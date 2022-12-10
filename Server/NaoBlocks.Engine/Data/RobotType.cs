using Newtonsoft.Json;

namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// Defines a robot type.
    /// </summary>
    public class RobotType
    {
        /// <summary>
        /// Gets or sets whether this robot type allows direct logging.
        /// </summary>
        public bool AllowDirectLogging { get; set; }

        /// <summary>
        /// Gets the allowed custom values.
        /// </summary>
        public IList<NamedValue> CustomValues { get; } = new List<NamedValue>();

        /// <summary>
        /// Gets or sets the id of the robot type.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this robot type is the default robot type for new users.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets the logging templates.
        /// </summary>
        public IList<LoggingTemplate> LoggingTemplates { get; private set; } = new List<LoggingTemplate>();

        /// <summary>
        /// Gets or sets a message associated with the robot.
        /// </summary>
        [JsonIgnore]
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets the name of the robot type.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets the toolboxes for this robot type.
        /// </summary>
        public IList<Toolbox> Toolboxes { get; private set; } = new List<Toolbox>();

        /// <summary>
        /// Gets or sets when the robot type was added.
        /// </summary>
        public DateTime WhenAdded { get; set; }
    }
}