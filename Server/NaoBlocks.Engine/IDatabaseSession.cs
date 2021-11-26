﻿using NaoBlocks.Engine.Data;

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
        /// Deletes an entity from the database.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity to delete.</param>
        void Delete<T>(T entity);
    }
}