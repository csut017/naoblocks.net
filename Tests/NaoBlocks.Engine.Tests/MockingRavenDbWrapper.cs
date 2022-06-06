using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Engine.Tests
{
    internal class MockingRavenDbWrapper : IDatabaseSession
    {
        private readonly Dictionary<string, object> cache = new();
        private object? lastModifiedEntity;
        private IAsyncDocumentSession? ravenDbSession;

        public MockingRavenDbWrapper(IAsyncDocumentSession? ravenDbSession)
        {
            this.ravenDbSession = ravenDbSession;
        }

        public bool DeletedCalled { get; private set; }

        public bool QueryCalled { get; private set; }

        public bool SavedChangedCalled { get; private set; }

        public bool StoreCalled { get; private set; }

        public void CacheItem<T>(string key, T item)
            where T : class
        {
            var fullKey = $"{typeof(T).FullName}->{key}";
            this.cache[fullKey] = item;
        }

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

        public T? GetFromCache<T>(string key)
                    where T : class
        {
            var fullKey = $"{typeof(T).FullName}->{key}";
            if (!this.cache.TryGetValue(fullKey, out var value))
            {
                return null;
            }

            return value as T;
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

        public IQueryable<T> Query<T>()
        {
            this.QueryCalled = true;
            if (this.ravenDbSession != null)
            {
                return this.ravenDbSession.Query<T>();
            }

            throw new NotImplementedException("Using query requires a RavenDB session");
        }

        public IQueryable<T> Query<T, TIndex>()
            where TIndex : AbstractCommonApiForIndexes, new()
        {
            this.QueryCalled = true;
            if (this.ravenDbSession != null)
            {
                return this.ravenDbSession.Query<T, TIndex>();
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
    }
}