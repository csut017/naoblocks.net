using System;
using System.Diagnostics.CodeAnalysis;

namespace NaoBlocks.Web.Helpers
{
    public class AppDatabaseOptions
    {
        public bool UseEmbedded { get; set; } = true;

        public string? DotNetPath { get; set; }

        public string? FrameworkVersion { get; set; }

        public string? Certificate { get; set; }

        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Configuration settings")]
        public string[] Urls { get; set; } = Array.Empty<string>();

        public string? Name { get; set; }
    }
}