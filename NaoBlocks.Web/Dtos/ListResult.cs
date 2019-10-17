using System.Collections.Generic;

namespace NaoBlocks.Web.Dtos
{
    public class ListResult<TData>
    {
        public int Count { get; set; }

        public IEnumerable<TData> Items { get; set; }

        public int Page { get; set; }
    }
}