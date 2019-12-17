using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Queries.Highlighting;
using Raven.Client.Documents.Session;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace RavenDB.Mocks
{
    public class FakeRavenQueryable<TItem>
        : IRavenQueryable<TItem>
    {
        private readonly IQueryable<TItem> _source;
        private readonly IQueryProvider _provider;
        private readonly DocumentOperations<TItem> _operations = new DocumentOperations<TItem>();

        public FakeRavenQueryable(IQueryable<TItem> source)
        {
            this._source = source;
            this._provider = new FakeRavenQueryProvider<TItem>(this._operations, source);
        }

        public DocumentOperations<TItem> Operations => this._operations;

        public Type ElementType => throw new NotImplementedException();

        public Expression Expression => this._source.Expression;

        public IQueryProvider Provider => this._provider;

        public IRavenQueryable<TItem> Customize(Action<IDocumentQueryCustomization> action)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public IRavenQueryable<TItem> Highlight(string fieldName, int fragmentLength, int fragmentCount, out Highlightings highlightings)
        {
            throw new NotImplementedException();
        }

        public IRavenQueryable<TItem> Highlight(string fieldName, int fragmentLength, int fragmentCount, HighlightingOptions options, out Highlightings highlightings)
        {
            throw new NotImplementedException();
        }

        public IRavenQueryable<TItem> Highlight(Expression<Func<TItem, object>> path, int fragmentLength, int fragmentCount, out Highlightings highlightings)
        {
            throw new NotImplementedException();
        }

        public IRavenQueryable<TItem> Highlight(Expression<Func<TItem, object>> path, int fragmentLength, int fragmentCount, HighlightingOptions options, out Highlightings highlightings)
        {
            throw new NotImplementedException();
        }

        public IRavenQueryable<TItem> Statistics(out QueryStatistics stats)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
