namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// Defines an item in a UI definition.
    /// </summary>
    public class UIDefinitionItem
    {
        /// <summary>
        /// Gets the children for this item.
        /// </summary>
        public IList<UIDefinitionItem> Children { get; private set; } = new List<UIDefinitionItem>();

        /// <summary>
        /// Gets or sets the description of the item.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the name of the item.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Generates a new definition item.
        /// </summary>
        /// <param name="name">The name of the item.</param>
        /// <param name="description">An optional description of the item.</param>
        /// <returns>The new <see cref="UIDefinitionItem"/> instance.</returns>
        public static UIDefinitionItem New(string name, string? description = null)
        {
            return new UIDefinitionItem
            {
                Name = name,
                Description = description
            };
        }

        /// <summary>
        /// Generates a new definition item with children.
        /// </summary>
        /// <param name="name">The name of the item.</param>
        /// <param name="description">An optional description of the item.</param>
        /// <param name="children">The children.</param>
        /// <returns>The new <see cref="UIDefinitionItem"/> instance.</returns>
        public static UIDefinitionItem New(string name, string? description, IEnumerable<UIDefinitionItem> children)
        {
            return new UIDefinitionItem
            {
                Name = name,
                Description = description,
                Children = new List<UIDefinitionItem>(children)
            };
        }
    }
}