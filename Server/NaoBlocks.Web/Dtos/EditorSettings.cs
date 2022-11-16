using Data = NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// A Data Transfer Object for a user's editor settings.
    /// </summary>
    public class EditorSettings
    {
        /// <summary>
        /// Gets or sets whether the editor can be condifured.
        /// </summary>
        public bool CanConfigure { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the system has been initialised.
        /// </summary>
        public bool IsSystemInitialised { get; set; }

        /// <summary>
        /// Gets or sets the toolbox for commands.
        /// </summary>
        public string? Toolbox { get; set; }

        /// <summary>
        /// Gets or sets whether the toolbox uses events.
        /// </summary>
        public bool UseEvents { get; set; }

        /// <summary>
        /// Gets or sets the user settings.
        /// </summary>
        public Data.UserSettings? User { get; set; }
    }
}