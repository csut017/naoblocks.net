using NaoBlocks.Engine.Data;
using Raven.Client.Documents.Indexes;

namespace NaoBlocks.Engine
{
    /// <summary>
    /// Defines the interface to a database session.
    /// </summary>
    public interface IDatabaseSession : IDisposable
    {
        /// <summary>
        /// Saves (commits) the changes to the database.
        /// </summary>
        Task SaveChangesAsync();

        /// <summary>
        /// Stores an entity.
        /// </summary>
        /// <param name="entity">The entity to store.</param>
        Task StoreAsync(object entity);

        /// <summary>
        /// Queries the database for one or more entities.
        /// </summary>
        /// <typeparam name="T">The type of data to query.</typeparam>
        /// <returns>An <see cref="IQueryable{T}"/> for retrieving the data.</returns>
        IQueryable<T> Query<T>();

        /// <summary>
        /// Queries the database for one or more entities using an index.
        /// </summary>
        /// <typeparam name="TIndex">The index to use.</typeparam>
        /// <typeparam name="T">The type of data to query.</typeparam>
        /// <returns>An <see cref="IQueryable{T}"/> for retrieving the data.</returns>
        IQueryable<T> Query<T, TIndex>()
            where TIndex : AbstractCommonApiForIndexes, new();

        /// <summary>
        /// Deletes an entity from the database.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity to delete.</param>
        void Delete<T>(T entity);

        /// <summary>
        /// Retrieves an entity by its identifier.
        /// </summary>
        /// <typeparam name="T">The type of entity to retrieve.</typeparam>
        /// <param name="id">The identifier of the entity.</param>
        /// <returns>The entity if found, null otherwise.</returns>
        Task<T?> LoadAsync<T>(string id);
    }
}