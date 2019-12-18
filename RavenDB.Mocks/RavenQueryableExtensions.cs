using System.Collections.Generic;
using System.Linq;

namespace RavenDB.Mocks
{
    public static class RavenQueryableExtensions
    {
        public static FakeRavenQueryable<TItem> AsRavenQueryable<TItem>(this IEnumerable<TItem> source)
        {
            return new FakeRavenQueryable<TItem>(source.AsQueryable());
        }
    }
}