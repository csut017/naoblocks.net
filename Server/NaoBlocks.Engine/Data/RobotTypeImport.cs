namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// Defines a robot type import definition.
    /// </summary>
    public class RobotTypeImport
    {
        /// <summary>
        /// Gets or sets whether this type is a duplicate.
        /// </summary>
        public bool IsDuplicate { get; set; }

        /// <summary>
        /// Gets or sets the associated robots.
        /// </summary>
        public List<Robot>? Robots { get; set; }

        /// <summary>
        /// Gets or sets the robot type.
        /// </summary>
        public RobotType? RobotType { get; set; }
    }
}