namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// Stores a user or robot session.
    /// </summary>
    public class Session
    {
        /// <summary>
        /// Gets or sets the identifier of the session.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets whether the session is for a robot or not.
        /// </summary>
        public bool IsRobot { get; set; }

        /// <summary>
        /// Gets or sets the user role.
        /// </summary>
        public UserRole? Role { get; set; }

        /// <summary>
        /// Gets or sets the user's identifier.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Gets or sets when the session was added.
        /// </summary>
        public DateTime WhenAdded { get; set; }

        /// <summary>
        /// Gets or sets when the session expires.
        /// </summary>
        public DateTime WhenExpires { get; set; }
    }
}
