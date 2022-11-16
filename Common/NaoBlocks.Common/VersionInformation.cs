namespace NaoBlocks.Common
{
    /// <summary>
    /// Contains the version information for the API.
    /// </summary>
    public class VersionInformation
    {
        /// <summary>
        /// Gets or sets the current API status.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current API version.
        /// </summary>
        public string Version { get; set; } = string.Empty;
    }
}
