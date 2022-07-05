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
        /// Gets or sets the allocated robot.
        /// </summary>
        public string? RobotId { get; set; }

        /// <summary>
        /// Gets or sets the name of the assigned robot type.
        /// </summary>
        public string? RobotType { get; set; }

        /// <summary>
        /// Gets or sets the id of the assigned robot type.
        /// </summary>
        public string? RobotTypeId { get; set; }

        /// <summary>
        /// Gets or sets the toolbox the user should use (if any.)
        /// </summary>
        public string? Toolbox { get; set; }

        /// <summary>
        /// Gets the optional toolbox categories to include.
        /// </summary>
        public IList<string> ToolboxCategories { get; private set; } = new List<string>();
    }
}