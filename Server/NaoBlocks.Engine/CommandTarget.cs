namespace NaoBlocks.Engine
{
    /// <summary>
    /// Defines the sub-system or sub-systems that a command targets.
    /// </summary>
    public enum CommandTarget
    {
        /// <summary>
        /// Targets the robot sub-system.
        /// </summary>
        Robot,

        /// <summary>
        /// Targets the robot log sub-system.
        /// </summary>
        RobotLog,

        /// <summary>
        /// Targets the robot type sub-system.
        /// </summary>
        RobotType,
    }
}