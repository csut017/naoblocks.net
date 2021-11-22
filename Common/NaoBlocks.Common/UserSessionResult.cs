namespace NaoBlocks.Common
{
    /// <summary>
    /// Contains the result of starting or resuming a user session.
    /// </summary>
    public class UserSessionResult
    {
        /// <summary>
        /// Gets or sets the date and time the session expires (in UTC.)
        /// </summary>
        public DateTime Expires { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets the role of the user.
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the session token.
        /// </summary>
        public string Token { get; set; } = string.Empty;
    }
}
