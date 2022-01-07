namespace NaoBlocks.Engine
{
    /// <summary>
    /// Defines the interface to the database.
    /// </summary>
    public interface IDatabase
    {
        /// <summary>
        /// Starts a new <see cref="IDatabaseSession"/> for working with a database.
        /// </summary>
        /// <returns>The new <see cref="IDatabaseSession"/> instance.</returns>
        IDatabaseSession StartSession();
    }
}