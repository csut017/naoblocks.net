using NaoBlocks.Engine.Data;
using Raven.Client.Documents.Session;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Engine.Tests
{
    internal class MockingRavenDbWrapper : IDatabaseSession
    {
        private IAsyncDocumentSession? ravenDbSession;
        private object? lastModifiedEntity;

        public MockingRavenDbWrapper(IAsyncDocumentSession? ravenDbSession)
        {
            this.ravenDbSession = ravenDbSession;
        }

        public bool DeletedCalled { get; private set; }

        public bool QueryCalled { get; private set; }

        public bool SavedChangedCalled { get; private set; }

        public bool StoreCalled { get; private set; }

        public void Delete<T>(T entity)
        {
            this.lastModifiedEntity = entity;
            this.DeletedCalled = true;
            if (this.ravenDbSession != null)
            {
                this.ravenDbSession.Delete(entity);
            }
        }

        public void Dispose()
        {
            if (this.ravenDbSession != null)
            {
                this.ravenDbSession.Dispose();
            }
        }

        public IQueryable<T> Query<T>()
        {
            this.QueryCalled = true;
            if (this.ravenDbSession != null)
            {
                return this.ravenDbSession.Query<T>();
            }

            throw new NotImplementedException("Using query requires a RavenDB session");
        }

        public async Task SaveChangesAsync()
        {
            this.SavedChangedCalled = true;
            if (this.ravenDbSession != null)
            {
                await this.ravenDbSession.SaveChangesAsync();
            }
        }

        public async Task StoreAsync(object entity)
        {
            this.lastModifiedEntity = entity;
            this.StoreCalled = true;
            if (this.ravenDbSession != null)
            {
                await this.ravenDbSession.StoreAsync(entity);
            }
        }

        public object? GetLastModifiedEntity()
        {
            return this.lastModifiedEntity;
        }

        public async Task<T?> LoadAsync<T>(string id)
        {
            if (this.ravenDbSession != null)
            {
                return await this.ravenDbSession.LoadAsync<T>(id);
            }

            throw new NotImplementedException("Using load requires a RavenDB session");
        }
    }
}