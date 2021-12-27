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
        public string? Certificate { get; set; }

        /// <summary>
        /// Gets or sets the path to the .Net framework.
        /// </summary>
        public string? DotNetPath { get; set; }

        /// <summary>
        /// Gets or sets the path to the embedded database.
        /// </summary>
        public string? EmbeddedPath { get; set; }

        /// <summary>
        /// Gets or sets the .Net framework to use.
        /// </summary>
        public string? FrameworkVersion { get; set; }

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
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