﻿namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// Defines a robot.
    /// </summary>
    public class Robot
    {
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
        /// Gets or sets the password the robot uses to connect.
        /// </summary>
        public Password Password { get; set; } = Password.Empty;

        /// <summary>
        /// Gets or sets the identifier of the robot type.
        /// </summary>
        public string RobotTypeId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the robot.
        /// </summary>
        public RobotType? Type { get; set; }

        /// <summary>
        /// Gets or sets when the robot was added.
        /// </summary>
        public DateTime WhenAdded { get; set; }
    }
}