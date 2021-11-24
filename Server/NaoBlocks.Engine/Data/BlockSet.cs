namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// Defines a blockset.
    /// </summary>
    public class BlockSet
    {
        /// <summary>
        /// The name of the blockset.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The categories the blockset has.
        /// </summary>
        public string BlockCategories { get; set; } = string.Empty;
    }
}