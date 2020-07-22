using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Embedded;
using System;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;

namespace NaoBlocks.Web.Helpers
{
    public static class DatabaseSupport
    {
        public static void AddRavenDb(this IServiceCollection services, AppSettings appSettings, IWebHostEnvironment env)
        {
            services.AddSingleton(serviceProvider =>
            {
                var logger = serviceProvider.GetService<ILogger<DatabaseInformation>>();
                IDocumentStore store;
                if ((appSettings.DatabaseOptions == null) || appSettings.DatabaseOptions.UseEmbedded)
                {
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

                    logger.LogInformation($"Embedded database can be accessed on {options.ServerUrl}");
                    logger.LogInformation("Starting embedded server");
                    EmbeddedServer.Instance.StartServer(options);
                    store = EmbeddedServer.Instance.GetDocumentStore("NaoBlocks");
                }
                else
                {
                    var certPath = appSettings.DatabaseOptions.Certificate ?? "certificate.pfx";
                    if (!Path.IsPathFullyQualified(certPath) && !certPath.StartsWith("~/", StringComparison.InvariantCulture))
                    {
                        certPath = Path.Combine(env.ContentRootPath, certPath);
                    }

                    logger.LogInformation($"Loading certificate from {certPath}");
                    var bytes = File.ReadAllBytes(certPath);
                    var cert = new X509Certificate2(bytes); 
                    logger.LogInformation($"Loaded certificate with subject {cert.Subject} [{cert.Thumbprint}]");

                    var dbName = appSettings.DatabaseOptions.Name ?? "NaoBlocks";
                    logger.LogInformation($"Connecting to remote RavenDB server:");
                    foreach (var url in appSettings.DatabaseOptions.Urls)
                    {
                        logger.LogInformation($"=> {url}");
                    }
                    logger.LogInformation($"Using database {dbName}");

                    store = new DocumentStore
                    {
                        Certificate = cert,
                        Database = dbName,
                        Urls = appSettings.DatabaseOptions.Urls.ToArray()
                    };
                }

                logger.LogInformation("Initialising database store");
                store.Initialize();

                logger.LogInformation("Generating indexes");
                IndexCreation.CreateIndexes(typeof(DatabaseSupport).Assembly, store);

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