namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// The details of a user.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the user's identifier.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the user's name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the next program number for the user.
        /// </summary>
        public long NextProgramNumber { get; set; }

        /// <summary>
        /// Gets or sets the user's password.
        /// </summary>
        public Password Password { get; set; } = Password.Empty;

        /// <summary>
        /// Gets or sets the user's role.
        /// </summary>
        public UserRole Role { get; set; }

        /// <summary>
        /// Gets or sets the user's settings.
        /// </summary>
        public UserSettings Settings { get; set; } = new UserSettings();

        /// <summary>
        /// Gets or sets when the user was added.
        /// </summary>
        public DateTime WhenAdded { get; set; }

        /// <summary>
        /// Gets or sets any associated student details.
        /// </summary>
        public StudentDetails? StudentDetails { get; set; }

        /// <summary>
        /// Gets or sets the user's last login token.
        /// </summary>
        public string? LoginToken { get; set; }
    }
}
