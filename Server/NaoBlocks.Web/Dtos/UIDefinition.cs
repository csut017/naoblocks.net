namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// A Data Transfer Object for a user.
    /// </summary>
    public class UIDefinition
    {
        /// <summary>
        /// Gets or sets the description of the UI.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the key of the UI.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the UI.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}