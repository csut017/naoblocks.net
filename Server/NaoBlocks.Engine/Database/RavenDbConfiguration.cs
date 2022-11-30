using System.Diagnostics.CodeAnalysis;

namespace NaoBlocks.Engine.Database
{
    /// <summary>
    /// Defines the configuration settings for a database connection.
    /// </summary>
    public class RavenDbConfiguration
    {
        /// <summary>
        /// Gets or sets the certificate to use when connecting to a non-embedded database.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public string? Certificate { get; set; }

        /// <summary>
        /// Gets or sets the data directory.
        /// </summary>
        public string? DataDirectory { get; set; }

        /// <summary>
        /// Gets or sets an optional server URL.
        /// </summary>
        public string? EmbeddedServerUrl { get; set; }

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the URLs to use when connecting to a non-embedded database.
        /// </summary>
        public string[] Urls { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets whether to use an embedded database instance.
        /// </summary>
        public bool UseEmbedded { get; set; } = true;
    }
}