namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// Defines a toolbox.
    /// </summary>
    public class Toolbox
    {
        /// <summary>
        /// Gets the categories.
        /// </summary>
        public IList<ToolboxCategory> Categories { get; private set; } = new List<ToolboxCategory>();

        /// <summary>
        /// Gets or sets whether this is the default toolbox.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets the name of the toolbox.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}