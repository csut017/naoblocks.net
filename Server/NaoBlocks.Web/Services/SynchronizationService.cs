using NaoBlocks.Engine;

namespace NaoBlocks.Web.Services
{
    /// <summary>
    /// A service for synchronizing servers.
    /// </summary>
    public class SynchronizationService
        : IService
    {
        private readonly IExecutionEngine executionEngine;
        private readonly ILogger<SynchronizationService> logger;

        /// <summary>
        /// Initialize a new <see cref="SynchronizationService"/> instance.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="executionEngine">The execution engine for processing commands and queries.</param>
        public SynchronizationService(ILogger<SynchronizationService> logger, IExecutionEngine executionEngine)
        {
            this.logger = logger;
            this.executionEngine = executionEngine;
        }

        /// <summary>
        /// Executes the service.
        /// </summary>
        public Task ExecuteAsync()
        {
            logger.LogInformation("Starting synchronization check");
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes the service.
        /// </summary>
        /// <returns>Returns the sleep period.</returns>
        public Task<TimeSpan> InitializeAsync()
        {
            // TODO: change this to configured value when everything is tested
            return Task.FromResult(TimeSpan.FromMinutes(1));
        }
    }
}