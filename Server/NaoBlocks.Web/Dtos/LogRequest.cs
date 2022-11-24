namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// Contains the details of a logging request.
    /// </summary>
    public class LogRequest
    {
        /// <summary>
        /// Gets or sets the type of log action.
        /// </summary>
        public string? Action { get; set; }

        /// <summary>
        /// Gets or sets the messages to log.
        /// </summary>
        public string[]? Messages { get; set; }

        /// <summary>
        /// Gets or sets the offset time of the request.
        /// </summary>
        public double? Time { get; set; }

        /// <summary>
        /// Gets or sets the robot program version.
        /// </summary>
        public string? Version { get; set; }
    }
}