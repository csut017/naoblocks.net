using System;
using System.Diagnostics.CodeAnalysis;

namespace NaoBlocks.Web.Helpers
{
    public class AppSettings
    {
        public AppDatabaseOptions? DatabaseOptions { get; set; }

        public string? JwtSecret { get; set; }
    }
}