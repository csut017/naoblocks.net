namespace NaoBlocks.Engine
{
    /// <summary>
    /// Provides additional metadata about a report format.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ReportFormatAttribute
        : Attribute
    {
        /// <summary>
        /// Initialises an instance of <see cref="ReportFormatAttribute"/>.
        /// </summary>
        /// <param name="mimeType">The MIME type of the format.</param>
        /// <param name="extension">The filename extension.</param>
        public ReportFormatAttribute(string mimeType, string extension)
        {
            MimeType = mimeType;
            Extension = extension;
        }

        /// <summary>
        /// Gets the filename extension.
        /// </summary>
        public string Extension { get; }

        /// <summary>
        /// Gets the MIME type.
        /// </summary>
        public string MimeType { get; }
    }
}