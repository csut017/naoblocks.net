namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// Defines a toolbox category.
    /// </summary>
    public class ToolboxCategory
    {
        /// <summary>
        /// Gets the blocks in the category.
        /// </summary>
        public IList<ToolboxBlock> Blocks { get; private set; } = new List<ToolboxBlock>();

        /// <summary>
        /// Gets or sets the colour of the category.
        /// </summary>
        public string Colour { get; set; } = "0";

        /// <summary>
        /// Gets or sets the custom components for the category.
        /// </summary>
        public string? Custom { get; set; }

        /// <summary>
        /// Gets or sets whether this category is optional or not.
        /// </summary>
        public bool IsOptional { get; set; }

        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}