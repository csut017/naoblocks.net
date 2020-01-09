using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Embedded;

namespace NaoBlocks.Web.Helpers
{
    public static class DatabaseSupport
    {
        public static void AddRavenDb(this IServiceCollection services, AppSettings appSettings)
        {
            services.AddSingleton(serviceProvider =>
            {
                var logger = serviceProvider.GetService<ILogger<DatabaseInformation>>();
                var options = new ServerOptions
                {
                    ServerUrl = "http://127.0.0.1:8088"
                };
                if (appSettings.DatabaseOptions != null)
                {
                    logger.LogInformation("Setting database options");
                    var dbOpts = appSettings.DatabaseOptions;
                    if (options.DotNetPath != null)
                    {
                        options.DotNetPath = dbOpts.DotNetPath;
                        logger.LogInformation($"=> DotNetPath={options.DotNetPath}");
                    }
                    if (options.FrameworkVersion != null)
                    {
                        options.FrameworkVersion = dbOpts.FrameworkVersion;
                        logger.LogInformation($"=> FrameworkVersion={options.FrameworkVersion}");
                    }
                }

                logger.LogInformation("Starting embedded server");
                EmbeddedServer.Instance.StartServer(options);
                var store = EmbeddedServer.Instance.GetDocumentStore("NaoBlocks");

                logger.LogInformation("Initialising database store");
                store.Initialize();

                logger.LogInformation("Generating indexes");
                IndexCreation.CreateIndexes(typeof(DatabaseSupport).Assembly, store);

                logger.LogInformation($"Embedded database can be access on {options.ServerUrl}");
                return store;
            });
            services.AddScoped(serviceProvider =>
            {
                var logger = serviceProvider.GetService<ILogger<DatabaseInformation>>();
                logger.LogInformation("Opening database session");
                return serviceProvider
                    .GetService<IDocumentStore>()
                    .OpenAsyncSession();
            });
        }

        internal class DatabaseInformation { }
    }
}