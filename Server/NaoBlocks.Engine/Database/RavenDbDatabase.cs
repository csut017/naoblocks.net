﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Embedded;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;

namespace NaoBlocks.Engine.Database
{
    /// <summary>
    /// Wrapper for RavenDB database interactions.
    /// </summary>
    public class RavenDbDatabase
        : IDatabase
    {
        private readonly ILogger<RavenDbDatabase> logger;
        private readonly IDocumentStore store;

        /// <summary>
        /// Initialise a new <see cref="RavenDbDatabase"/> instance.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="store">The <see cref="IDocumentStore"/> instance.</param>
        public RavenDbDatabase(ILogger<RavenDbDatabase> logger, IDocumentStore store)
        {
            this.logger = logger;
            this.store = store;
        }

        /// <summary>
        /// Initialises a new instance of a <see cref="IDatabase"/> using RavenDB.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The configuration settings to use.</param>
        /// <param name="certificatePath">The path to any certificates.</param>
        /// <returns>The new <see cref="IDatabase"/> instance.</returns>
        public static async Task<IDatabase> New(
            ILogger<RavenDbDatabase> logger,
            RavenDbConfiguration? configuration,
            string certificatePath)
        {
            logger.LogInformation("Initialising database store");
            var store = (configuration == null) || configuration.UseEmbedded
                ? InitialiseEmbeddedDatabase(logger, configuration)
                : await InitialiseRemoteDatabase(logger, configuration, certificatePath);

            logger.LogInformation("Starting database store");
            store.Initialize();

            logger.LogInformation("Generating indices");
            var timer = new Stopwatch();
            timer.Start();
            IndexCreation.CreateIndexes(typeof(RavenDbDatabase).Assembly, store);
            timer.Stop();
            logger.LogInformation("Indices generated in {time:#,##0}ms", timer.Elapsed.TotalMilliseconds);
            return new RavenDbDatabase(logger, store);
        }

        /// <summary>
        /// Start the database in a background thread.
        /// </summary>
        /// <param name="services">The <see cref="IServiceProvider"/> to use.</param>
        public static void Start(IServiceProvider services)
        {
            Task.Run(() =>
            {
                var logger = services.GetRequiredService<ILogger<RavenDbDatabase>>();
                logger.LogInformation("Starting database");
                var timer = new Stopwatch();
                timer.Start();
                services.GetService<IDatabase>();
                timer.Stop();
                logger.LogInformation("Database started in {time:#,##0}ms", timer.Elapsed.TotalMilliseconds);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Defines the interface to the database.
        /// </summary>
        public IDatabaseSession StartSession()
        {
            this.logger.LogDebug("Starting a new database session.");
            var session = this.store.OpenAsyncSession();
            return new RavenDbDatabaseSession(session);
        }

        private static IDocumentStore InitialiseEmbeddedDatabase(ILogger<RavenDbDatabase> logger, RavenDbConfiguration? configuration)
        {
            IDocumentStore store;
            var options = new ServerOptions
            {
                ServerUrl = configuration?.EmbeddedServerUrl ?? "http://127.0.0.1:8090"
            };
            if (configuration != null)
            {
                logger.LogInformation("Setting database options");
                if (!string.IsNullOrWhiteSpace(configuration.DataDirectory))
                {
                    options.DataDirectory = configuration.DataDirectory;
                    logger.LogInformation("=> DataDirectory={directory}", options.DataDirectory);
                }
            }

            logger.LogInformation("Embedded database can be accessed on {url}", options.ServerUrl);
            logger.LogInformation("Starting embedded server");
            var timer = new Stopwatch();
            timer.Start();
            EmbeddedServer.Instance.StartServer(options);
            timer.Stop();
            logger.LogInformation("Server started in {time:#,##0}ms", timer.Elapsed.TotalMilliseconds);
            logger.LogInformation("Getting document store");
            timer.Restart();
            store = EmbeddedServer.Instance.GetDocumentStore("NaoBlocks");
            timer.Stop();
            logger.LogInformation("Store retrieved in {time:#,##0}ms", timer.Elapsed.TotalMilliseconds);
            return store;
        }

        [ExcludeFromCodeCoverage(Justification = "This method requires connecting to an external database server")]
        private static async Task<IDocumentStore> InitialiseRemoteDatabase(ILogger<RavenDbDatabase> logger, RavenDbConfiguration? configuration, string certificatePath)
        {
            IDocumentStore store;
            var certPath = configuration?.Certificate ?? "certificate.pfx";
            if (!Path.IsPathFullyQualified(certPath) && !certPath.StartsWith("~/", StringComparison.InvariantCulture))
            {
                certPath = Path.Combine(certificatePath, certPath);
            }

            logger.LogInformation("Loading certificate from {path}", certPath);
            var bytes = await File.ReadAllBytesAsync(certPath);
            var cert = new X509Certificate2(bytes);
            logger.LogInformation("Loaded certificate with subject {subject} [{thumbprint}]", cert.Subject, cert.Thumbprint);

            var dbName = configuration?.Name ?? "NaoBlocks";
            logger.LogInformation("Connecting to remote RavenDB server:");
            var urls = configuration?.Urls ?? Array.Empty<string>();
            foreach (var url in urls)
            {
                logger.LogInformation("=> {url}", url);
            }
            logger.LogInformation("Using database {name}", dbName);

            store = new DocumentStore
            {
                Certificate = cert,
                Database = dbName,
                Urls = urls
            };
            return store;
        }
    }
}