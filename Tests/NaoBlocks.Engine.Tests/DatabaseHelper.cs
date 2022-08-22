using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Session;
using Raven.TestDriver;
using System;
using System.Threading.Tasks;

namespace NaoBlocks.Engine.Tests
{
    public class DatabaseHelper : RavenTestDriver
    {
        public IDocumentStore InitialiseDatabase(Action<IDocumentSession>? initialiseData = null)
        {
            var store = GetDocumentStore();
            if (initialiseData == null) return store;

            using var session = store.OpenSession();
            initialiseData(session);
            session.SaveChanges();
            WaitForIndexing(store);
            return store;
        }

        public IDocumentStore InitialiseDatabase(params object[] entities)
        {
            var store = GetDocumentStore();
            using var session = store.OpenSession();
            foreach (var entity in entities)
            {
                session.Store(entity);
            }
            session.SaveChanges();
            WaitForIndexing(store);
            return store;
        }

        protected static TGenerator InitialiseGenerator<TGenerator>(IAsyncDocumentSession session)
            where TGenerator : ReportGenerator, new()
        {
            var query = new TGenerator();
            query.InitialiseSession(WrapSession(session));
            return query;
        }

        protected static TQuery InitialiseQuery<TQuery>(IAsyncDocumentSession session)
            where TQuery : DataQuery, new()
        {
            var query = new TQuery();
            query.InitialiseSession(WrapSession(session));
            return query;
        }

        protected static IDatabaseSession WrapSession(IAsyncDocumentSession session)
        {
            return new MockingRavenDbWrapper(session);
        }

        protected async Task InitialiseIndicesAsync(IDocumentStore store, params IAbstractIndexCreationTask[] indices)
        {
            foreach (var index in indices)
            {
                await index.ExecuteAsync(store);
            }

            WaitForIndexing(store);
        }
    }
}