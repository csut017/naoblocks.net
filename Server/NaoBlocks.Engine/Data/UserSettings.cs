namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// The user's settings when using the application.
    /// </summary>
    public class UserSettings
    {
        /// <summary>
        /// Gets or sets the robot allocation mode.
        /// </summary>
        /// <remarks>
        /// 0 = random
        /// 1 = assigned robot
        /// </remarks>
        public int AllocationMode { get; set; }

        /// <summary>
        /// Gets or sets whether the user can use conditional blocks.
        /// </summary>
        public bool Conditionals { get; set; }

        /// <summary>
        /// Gets or sets the custom blockset the user should use (if any.)
        /// </summary>
        public string? CustomBlockSet { get; set; }

        /// <summary>
        /// Gets or sets whether the user can use dance blocks.
        /// </summary>
        public bool Dances { get; set; }

        /// <summary>
        /// Gets or sets whether the user can use event blocks.
        /// </summary>
        public bool Events { get; set; }

        /// <summary>
        /// Gets or sets whether the user can use loop blocks.
        /// </summary>
        public bool Loops { get; set; }

        /// <summary>
        /// Gets or sets the name of the assigned robot type.
        /// </summary>
        public string? RobotType { get; set; }

        /// <summary>
        /// Gets or sets the id of the assigned robot type.
        /// </summary>
        public string? RobotTypeId { get; set; }

        /// <summary>
        /// Gets or sets the allocated robot.
        /// </summary>
        public string? RobotId { get; set; }

        /// <summary>
        /// Gets or sets whether the user can use sensor blocks.
        /// </summary>
        public bool Sensors { get; set; }

        /// <summary>
        /// Gets or sets whether the user is in simple mode or not.
        /// </summary>
        public bool Simple { get; set; }

        /// <summary>
        /// Gets or sets whether the user can use variable blocks.
        /// </summary>
        public bool Variables { get; set; }
    }
}