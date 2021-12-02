namespace NaoBlocks.Web.Communications
{
    /// <summary>
    /// Defines the type of client connection.
    /// </summary>
    public enum ClientConnectionType
    {
        /// <summary>
        /// An unknown connection.
        /// </summary>
        Unknown,

        /// <summary>
        /// The client is a robot.
        /// </summary>
        Robot,

        /// <summary>
        /// The client is a user.
        /// </summary>
        User
    }
}