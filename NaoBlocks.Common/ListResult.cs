using System.Collections.Generic;
using System.Linq;

namespace NaoBlocks.Common
{
    public class ListResult<TData>
    {
        public int Count { get; set; }

        public IEnumerable<TData>? Items { get; set; }

        public int Page { get; set; }
    }

    public static class ListResult
    {
        public static ListResult<TData> New<TData>(IEnumerable<TData> items)
        {
            return new ListResult<TData>
            {
                Items = items,
                Count = items.Count()
            };
        }
    }
}