namespace NaoBlocks.Web.Services
{
    /// <summary>
    /// Infrastructure for periodicly running services.
    /// </summary>
    public class PeriodicHostedService<TService>
        : BackgroundService
        where TService : class, IService
    {
        private readonly IServiceScopeFactory factory;
        private readonly ILogger<PeriodicHostedService<TService>> logger;

        /// <summary>
        /// Initializes a new <see cref="PeriodicHostedService{TService}"/> instance.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="factory">The <see cref="IServiceScopeFactory"/> to use.</param>
        public PeriodicHostedService(ILogger<PeriodicHostedService<TService>> logger, IServiceScopeFactory factory)
        {
            this.logger = logger;
            this.factory = factory;
        }

        /// <summary>
        /// Main loop for running the service.
        /// </summary>
        /// <param name="stoppingToken">A <see cref="CancellationToken"/> for halting execution.</param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var name = typeof(TService).Name;
            logger.LogInformation("Initializing periodic background service {name}", name);
            var sleepPeriod = await InitializeAsync();

            logger.LogInformation("Started periodic background service {name}", name);
            using var timer = new PeriodicTimer(sleepPeriod);
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await using var scope = factory.CreateAsyncScope();
                    var compileService = scope.ServiceProvider.GetRequiredService<TService>();
                    await compileService.ExecuteAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Periodic background service {name} failed", name);
                }
            }
        }

        /// <summary>
        /// Performs the start-up initialization for the service.
        /// </summary>
        private async Task<TimeSpan> InitializeAsync()
        {
            await using var scope = factory.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            return await service.InitializeAsync();
        }
    }
}