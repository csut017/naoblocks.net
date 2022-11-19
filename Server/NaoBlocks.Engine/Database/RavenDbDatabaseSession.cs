using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Session;

namespace NaoBlocks.Engine.Database
{
    /// <summary>
    /// A wrapper class for holding the RavenDB document session.
    /// </summary>
    public class RavenDbDatabaseSession
        : IDatabaseSession
    {
        private readonly Dictionary<string, object> cache = new();
        private readonly IAsyncDocumentSession session;
        private bool disposedValue;

        /// <summary>
        /// Starts a new <see cref="RavenDbDatabaseSession"/> instance.
        /// </summary>
        /// <param name="session">The underlying <see cref="IAsyncDocumentSession"/> instance.</param>
        public RavenDbDatabaseSession(IAsyncDocumentSession session)
        {
            this.session = session;
        }

        /// <summary>
        /// Caches an item that can be used within a command.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="key">The key of the item.</param>
        /// <param name="item">The item to cache.</param>
        public void CacheItem<T>(string key, T item)
            where T : class
        {
            var fullKey = $"{typeof(T).FullName}->{key}";
            this.cache[fullKey] = item;
        }

        /// <summary>
        /// Deletes an entity from the database.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity to delete.</param>
        public void Delete<T>(T entity)
        {
            this.session.Delete(entity);
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Retrieve an item from the command cache.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="key">The key of the item.</param>
        /// <returns>The item, if found, otherwise null.</returns>
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

        /// <summary>
        /// Retrieves an entity by its identifier.
        /// </summary>
        /// <typeparam name="T">The type of entity to retrieve.</typeparam>
        /// <param name="id">The identifier of the entity.</param>
        /// <returns>The entity if found, null otherwise.</returns>
        public async Task<T?> LoadAsync<T>(string id)
        {
            return await this.session.LoadAsync<T>(id);
        }

        /// <summary>
        /// Queries the database for one or more entities.
        /// </summary>
        /// <typeparam name="T">The type of data to query.</typeparam>
        /// <returns>An IQueryable for retrieving the data.</returns>
        public IQueryable<T> Query<T>()
        {
            return this.session.Query<T>();
        }

        /// <summary>
        /// Queries the database for one or more entities using an index.
        /// </summary>
        /// <typeparam name="TIndex">The index to use.</typeparam>
        /// <typeparam name="T">The type of data to query.</typeparam>
        /// <returns>An IQueryable for retrieving the data.</returns>
        public IQueryable<T> Query<T, TIndex>()
            where TIndex : AbstractCommonApiForIndexes, new()
        {
            return this.session.Query<T, TIndex>();
        }

        /// <summary>
        /// Saves (commits) the changes to the database.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await this.session.SaveChangesAsync();
        }

        /// <summary>
        /// Stores an entity.
        /// </summary>
        /// <param name="entity">The entity to store.</param>
        public async Task StoreAsync(object entity)
        {
            await this.session.StoreAsync(entity);
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        /// <param name="disposing">True if managed states needs to be disposed, false otherwise.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.session.Dispose();
                }

                disposedValue = true;
            }
        }
    }
}