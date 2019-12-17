using System;
using System.Linq;

namespace RavenDB.Mocks
{
    public class DocumentOperations<TItem>
    {
        public Func<IQueryable<TItem>, bool> Any { get; set; } = s => s.Any();

        public Func<IQueryable<TItem>, TItem> FirstOrDefault { get; set; } = s => s.FirstOrDefault();
    }
}