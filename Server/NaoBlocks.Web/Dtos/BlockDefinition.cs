namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// A data transfer object for a block definition.
    /// </summary>
    public class BlockDefinition
    {
        /// <summary>
        /// Gets or sets the category the block belongs to.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Gets or sets the colour of the block.
        /// </summary>
        public string? Colour { get; set; }

        /// <summary>
        /// Gets or sets the name of the block.
        /// </summary>
        public string? Name { get; set; }
    }
}