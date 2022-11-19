using Data = NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// A Data Transfer Object for a robot.
    /// </summary>
    public class Robot
    {
        /// <summary>
        /// Gets or sets the robot's friendly (human-readable) name.
        /// </summary>
        public string FriendlyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the robot is initialised.
        /// </summary>
        public bool IsInitialised { get; set; }

        /// <summary>
        /// Gets or sets the robot's machine name.
        /// </summary>
        public string MachineName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a message associated with the robot.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets the robot's password.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the robot type.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets when the robot was added.
        /// </summary>
        public DateTime? WhenAdded { get; set; }

        /// <summary>
        /// Converts a database entity to a Data Transfer Object.
        /// </summary>
        /// <param name="value">The database entity.</param>
        /// <returns>A new <see cref="Robot"/> instance containing the required properties.</returns>
        public static Robot FromModel(Data.Robot value)
        {
            var type = value.Type?.Name ?? value.RobotTypeId;
            return new Robot
            {
                FriendlyName = value.FriendlyName,
                IsInitialised = value.IsInitialised,
                MachineName = value.MachineName,
                Message = value.Message,
                Password = value.PlainPassword,
                Type = string.IsNullOrEmpty(type) ? null : type,
                WhenAdded = value.WhenAdded,
            };
        }
    }
}