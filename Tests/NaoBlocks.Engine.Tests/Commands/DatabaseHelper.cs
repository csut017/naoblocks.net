using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.TestDriver;
using System;

namespace NaoBlocks.Engine.Tests.Commands
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
    }
}
