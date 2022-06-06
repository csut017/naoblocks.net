namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// A Data Transfer Object containing the system initialisation settings.
    /// </summary>
    public class InitialisationSettings
    {
        /// <summary>
        /// Gets or sets whether the system should use the default Nao robot definition.
        /// </summary>
        public bool AddNaoRobot { get; set; }

        /// <summary>
        /// Gets or sets the admin user's password.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets whether the system should use the default Use Interface components.
        /// </summary>
        public bool UseDefaultUi { get; set; }
    }
}