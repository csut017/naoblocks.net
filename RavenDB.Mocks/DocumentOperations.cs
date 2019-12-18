using Raven.Client.Documents.Session;
using System;
using System.Linq;

namespace RavenDB.Mocks
{
    public class DocumentOperations<TItem>
    {
        public Func<IQueryable<TItem>, bool> Any { get; set; } = s => s.Any();

        public Func<IQueryable<TItem>, TItem> FirstOrDefault { get; set; } = s => s.FirstOrDefault();

        public void Statistics(IQueryable<TItem> source, out QueryStatistics stats)
        {
            stats = new QueryStatistics { TotalResults = source.Count() };
        }
    }
}