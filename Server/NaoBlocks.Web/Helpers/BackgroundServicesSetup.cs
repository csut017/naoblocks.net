using NaoBlocks.Web.Services;

namespace NaoBlocks.Web.Helpers
{
    /// <summary>
    /// Helper methods for background services
    /// </summary>
    public static class BackgroundServicesSetup
    {
        /// <summary>
        /// Adds a periodic background service.
        /// </summary>
        /// <typeparam name="T">The type of the service to add.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        public static void AddBackgroundService<T>(this IServiceCollection services)
            where T : class, IService
        {
            services.AddScoped<T>();
            services.AddSingleton<PeriodicHostedService<T>>();
            services.AddHostedService(provider => provider.GetRequiredService<PeriodicHostedService<T>>());
        }
    }
}