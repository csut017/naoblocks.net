namespace NaoBlocks.Web.Communications
{
    /// <summary>
    /// Contains information on the last program execution by a robot.
    /// </summary>
    public class RobotStatus
    {
        /// <summary>
        /// Gets or sets the last program identifier.
        /// </summary>
        public int? LastProgramId { get; set; }

        /// <summary>
        /// Gets the source IDs that have been executed.
        /// </summary>
        public IList<string> SourceIds { get; } = new List<string>();
    }
}