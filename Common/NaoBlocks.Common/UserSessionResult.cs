namespace NaoBlocks.Common
{
    /// <summary>
    /// Contains the result of starting or resuming a user session.
    /// </summary>
    public class UserSessionResult
    {
        /// <summary>
        /// Gets or sets how long until the session expires in minutes.
        /// </summary>
        public int TimeRemaining { get; set; }

        /// <summary>
        /// Gets or sets the role of the user.
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the session token.
        /// </summary>
        public string Token { get; set; } = string.Empty;
    }
}