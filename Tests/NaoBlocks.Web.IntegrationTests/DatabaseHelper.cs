using Raven.Client.Documents;
using Raven.TestDriver;

namespace NaoBlocks.Web.IntegrationTests
{
    public class DatabaseHelper : RavenTestDriver
    {
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
