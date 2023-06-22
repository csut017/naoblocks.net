namespace NaoBlocks.Web.Services
{
    /// <summary>
    /// Defines a periodic background service.
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// Executes the service.
        /// </summary>
        Task ExecuteAsync();

        /// <summary>
        /// Initializes the service.
        /// </summary>
        /// <returns>Returns the sleep period.</returns>
        Task<TimeSpan> InitializeAsync();
    }
}