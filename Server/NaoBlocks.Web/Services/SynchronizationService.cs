using NaoBlocks.Engine;
using NaoBlocks.Engine.Queries;

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
        public async Task ExecuteAsync()
        {
            logger.LogInformation("Starting synchronization check");

            var sources = await executionEngine.Query<SynchronizationData>()
                .RetrieveSourcePageAsync(0, 50)
                .ConfigureAwait(false);
            if (sources.Items == null)
            {
                logger.LogInformation("No sources configured - skipping synchronization");
                return;
            }

            var success = 0;
            var failed = 0;
            foreach (var source in sources.Items)
            {
                var token = source.Tokens?.FirstOrDefault(t => t.Machine == Environment.MachineName);
                if (token?.Token == null)
                {
                    failed++;
                    logger.LogWarning("Source {source} does not have a valid token - skipping", source.Name);
                    continue;
                }

                var tokenValue = ProtectedDataUtility.DecryptValue(token.Token);
                var url = $"{source.Address}/api/v1/";
                var client = new HttpClient { BaseAddress = new Uri(url) };
                logger.LogInformation("Authenticating to {name}", source.Name);
                var connectResponse = await client.PostAsJsonAsync("session", new Dtos.User { Token = tokenValue.Trim() });
                if (!connectResponse.IsSuccessStatusCode)
                {
                    failed++;
                    logger.LogWarning("Failed to authenticate with {source} - skipping", source.Name);
                    continue;
                }
            }

            logger.LogInformation("Synchronization check finished - {success} succeeded, {failed} failed", success, failed);
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