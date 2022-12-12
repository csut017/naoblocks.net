namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// Records an item import.
    /// </summary>
    public static class ItemImport
    {
        /// <summary>
        /// Generates a new <see cref="ItemImport{TItem}"/> instance.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="item">The item.</param>
        /// <param name="row">An optional row to associate with the item.</param>
        /// <param name="message">An optional message to associate with the item.</param>
        /// <returns>A new <see cref="ItemImport{TItem}"/> instance.</returns>
        public static ItemImport<TItem> New<TItem>(TItem item, int row = 0, string? message = null)
            where TItem : class
        {
            return new ItemImport<TItem>
            {
                Item = item,
                Row = row,
                Message = message,
            };
        }
    }

    /// <summary>
    /// Records an item import.
    /// </summary>
    public class ItemImport<TItem>
        where TItem : class
    {
        /// <summary>
        /// Gets or sets whether this item is a duplicate.
        /// </summary>
        public bool IsDuplicate { get; set; }

        /// <summary>
        /// Gets or sets the imported item.
        /// </summary>
        public TItem? Item { get; set; }

        /// <summary>
        /// Gets or sets a message associated with the item.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets the row the item was imported from.
        /// </summary>
        public int Row { get; set; }
    }
}