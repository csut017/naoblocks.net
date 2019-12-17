using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Queries;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace RavenDB.Mocks
{
    public class FakeRavenQueryProvider<TItem>
        : IRavenQueryProvider
    {
        private readonly DocumentOperations<TItem> _operations;
        private readonly IQueryable<TItem> _source;
        private readonly IQueryProvider _provider;

        public FakeRavenQueryProvider(DocumentOperations<TItem> operations, IQueryable<TItem> source)
        {
            this._operations = operations;
            this._source = source;
            this._provider = source.Provider;
        }

        public Action<IDocumentQueryCustomization> CustomizeQuery => throw new NotImplementedException();
        public HashSet<FieldToFetch> FieldsToFetch => throw new NotImplementedException();
        public string IndexName => throw new NotImplementedException();

        public bool IsProjectInto { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Type OriginalQueryType => throw new NotImplementedException();
        public IDocumentQueryGenerator QueryGenerator => throw new NotImplementedException();

        public void AfterQueryExecuted(Action<QueryResult> afterQueryExecuted)
        {
            throw new NotImplementedException();
        }

        public Lazy<int> CountLazily<T>(Expression expression)
        {
            throw new NotImplementedException();
        }

        public Lazy<Task<int>> CountLazilyAsync<T>(Expression expression, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return this._provider.CreateQuery(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return this._provider.CreateQuery<TElement>(expression);
        }

        public void Customize(Action<IDocumentQueryCustomization> action)
        {
            throw new NotImplementedException();
        }

        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            throw new NotImplementedException();
        }

        public IRavenQueryProvider For<TS>()
        {
            throw new NotImplementedException();
        }

        public Lazy<IEnumerable<T>> Lazily<T>(Expression expression, Action<IEnumerable<T>> onEval)
        {
            throw new NotImplementedException();
        }

        public Lazy<Task<IEnumerable<T>>> LazilyAsync<T>(Expression expression, Action<IEnumerable<T>> onEval)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<T> ToAsyncDocumentQuery<T>(Expression expression)
        {
            var doc = new FakeAsyncDocumentQuery<T>(this._operations as DocumentOperations<T>, this._source as IQueryable<T>);
            return doc;
        }

        public IDocumentQuery<TResult> ToDocumentQuery<TResult>(Expression expression)
        {
            throw new NotImplementedException();
        }
    }
}