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
    }
}