using Newtonsoft.Json;

namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// Defines a robot.
    /// </summary>
    public class Robot
    {
        /// <summary>
        /// Gets the current custom values.
        /// </summary>
        public IList<NamedValue> CustomValues { get; } = new List<NamedValue>();

        /// <summary>
        /// Gets or sets the human-friendly name of the robot.
        /// </summary>
        public string FriendlyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the identifier of the robot.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the robot has been initialised and can be used in the system.
        /// </summary>
        public bool IsInitialised { get; set; }

        /// <summary>
        /// Gets or sets the machine name of the robot.
        /// </summary>
        public string MachineName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a message associated with the robot.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets the password the robot uses to connect.
        /// </summary>
        public Password Password { get; set; } = Password.Empty;

        /// <summary>
        /// Gets or sets the plain text password.
        /// </summary>
        /// <remarks>
        /// This property will not be persisted, it is only used for internal passing of the password.
        /// </remarks>
        [JsonIgnore]
        public string? PlainPassword { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the robot type.
        /// </summary>
        public string RobotTypeId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the robot.
        /// </summary>
        [JsonIgnore]
        public RobotType? Type { get; set; }

        /// <summary>
        /// Gets or sets when the robot was added.
        /// </summary>
        public DateTime WhenAdded { get; set; }
    }
}