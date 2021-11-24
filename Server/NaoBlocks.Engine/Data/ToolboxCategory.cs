namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// Defines a toolbox category.
    /// </summary>
    public class ToolboxCategory
    {
        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the colour of the category.
        /// </summary>
        public string Colour { get; set; } = "0";

        /// <summary>
        /// Gets or sets the custom components for the category.
        /// </summary>
        public string? Custom { get; set; }

        /// <summary>
        /// Gets or sets the order of the category.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets the tags of the category.
        /// </summary>
        public IList<string> Tags { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the blocks in the category.
        /// </summary>
        public IList<ToolboxBlock> Blocks { get; private set; } = new List<ToolboxBlock>();
    }
}