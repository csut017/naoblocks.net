using System;
using System.Linq;

namespace RavenDB.Mocks
{
    public class DocumentOperations<TItem>
    {
        public Func<IQueryable<TItem>, bool> Any { get; set; } = s => false;
    }
}