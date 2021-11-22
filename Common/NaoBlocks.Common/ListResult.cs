namespace NaoBlocks.Common
{
    /// <summary>
    /// A result containing a list of items.
    /// </summary>
    /// <typeparam name="TData">The type of data contained in the list.</typeparam>
    public class ListResult<TData>
    {
        /// <summary>
        /// The total number of items included in the resultset.
        /// </summary>
        /// <remarks>
        /// This count may be larger than the number of items in the resultset if paging is used.
        /// </remarks>
        public int Count { get; set; }

        /// <summary>
        /// The data items.
        /// </summary>
        public IEnumerable<TData>? Items { get; set; }

        /// <summary>
        /// The page number within the resultset.
        /// </summary>
        public int Page { get; set; }
    }

    /// <summary>
    /// Helper class for generating <see cref="ListResult{TData}"/> instances.
    /// </summary>
    public static class ListResult
    {
        /// <summary>
        /// Generates a new <see cref="ListResult{TData}"/> instance.
        /// </summary>
        /// <typeparam name="TData">The type of data the result will contain.</typeparam>
        /// <param name="items">The data items.</param>
        /// <param name="count">The count of items in the resultset. If null, then the number of items will be the count.</param>
        /// <param name="page">The page number. If null, then the first page (0).</param>
        /// <returns>A <see cref="ListResult{TData}"/> containing the items.</returns>
        public static ListResult<TData> New<TData>(IEnumerable<TData> items, int? count = null, int? page = null)
        {
            return new ListResult<TData>
            {
                Items = items,
                Page = page ?? 0,
                Count = count ?? items.Count()
            };
        }
    }
}
