namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// Defines a set of records.
    /// </summary>
    public static class Set
    {
        /// <summary>
        /// Generates a new <see cref="Set{TItem}"/> from an enumerable.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="items">The items.</param>
        /// <returns>The new <see cref="Set{TItem}"/> instance.</returns>
        public static Set<TItem> New<TItem>(IEnumerable<TItem> items)
        {
            return new Set<TItem>(items);
        }

        /// <summary>
        /// Generates a new <see cref="Set{TItem}"/> from an array.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="items">The items.</param>
        /// <returns>The new <see cref="Set{TItem}"/> instance.</returns>
        public static Set<TItem> New<TItem>(params TItem[] items)
        {
            return new Set<TItem>(items);
        }
    }

    /// <summary>
    /// Defines a set of items.
    /// </summary>
    /// <typeparam name="TItem">The items.</typeparam>
    public class Set<TItem>
    {
        /// <summary>
        /// Initialises a new <see cref="Set{TItem}"/> instance.
        /// </summary>
        public Set()
        {
        }

        /// <summary>
        /// Initialises a new <see cref="Set{TItem}"/> instances from an existing set of items.
        /// </summary>
        /// <param name="items">The items.</param>
        public Set(IEnumerable<TItem> items)
        {
            this.Items = items;
        }

        /// <summary>
        /// Gets or sets the items in the set.
        /// </summary>
        public IEnumerable<TItem>? Items { get; set; }
    }
}