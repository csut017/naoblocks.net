namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// A data transfer object for a block definition.
    /// </summary>
    public class BlockDefinition
    {
        /// <summary>
        /// Gets the categories the block belongs to.
        /// </summary>
        public IEnumerable<string> Categories { get; } = new List<string>();

        /// <summary>
        /// Gets or sets the name of the block.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Adds a category to the list of categories.
        /// </summary>
        /// <param name="name">The name of the category.</param>
        public void AddCategory(string name)
        {
            ((List<string>)this.Categories).Add(name);
        }
    }
}