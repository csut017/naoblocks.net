namespace NaoBlocks.Web.Communications
{
    /// <summary>
    /// Marks an item as startable.
    /// </summary>
    public interface IStartableClientConnection
        : IClientConnection
    {
        /// <summary>
        /// Starts the instance's internal process.
        /// </summary>
        public Task StartAsync();
    }
}