using System;
using System.Diagnostics.CodeAnalysis;

namespace NaoBlocks.Web.Helpers
{
    public class AppSettings
    {
        public string? DatabaseName { get; set; }

        public AppDatabaseOptions? DatabaseOptions { get; set; }

        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Configuration settings")]
        public string[] DatabaseUrls { get; set; } = Array.Empty<string>();

        public string? JwtSecret { get; set; }
    }
}