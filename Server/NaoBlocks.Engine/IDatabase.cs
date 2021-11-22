namespace NaoBlocks.Engine
{
    /// <summary>
    /// Defines the interface to the database.
    /// </summary>
    public interface IDatabase
    {
        IDatabaseSession StartSession();
    }
}