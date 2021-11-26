using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.TestDriver;
using System;

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

        protected static IDatabaseSession WrapSession(IAsyncDocumentSession session)
        {
            return new MockingRavenDbWrapper(session);
        }

        protected static TQuery InitialiseQuery<TQuery>(IAsyncDocumentSession session)
            where TQuery : DataQuery, new()
        {
            var query = new TQuery();
            query.InitialiseSession(WrapSession(session));
            return query;
        }
    }
}
