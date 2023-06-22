using NaoBlocks.Engine;

namespace NaoBlocks.Web.Helpers
{
    /// <summary>
    /// Defines some common MIME types.
    /// </summary>
    public static class ContentTypes
    {
        /// <summary>
        /// The file type for a plain text file.
        /// </summary>
        public const string PlainText = "text/plain";

        /// <summary>
        /// The file type for a PNG image file.
        /// </summary>
        public const string Png = "image/png";

        private static readonly Lazy<Dictionary<ReportFormat, string>> _contentTypes = new(() =>
                {
                    var contentTypes = new Dictionary<ReportFormat, string>();
                    var fields = typeof(ReportFormat).GetFields();
                    foreach (var field in fields)
                    {
                        if (field.GetCustomAttributes(typeof(ReportFormatAttribute), false).FirstOrDefault() is ReportFormatAttribute attrib)
                        {
                            var key = (ReportFormat)field.GetValue(null)!;
                            contentTypes.Add(key, attrib.MimeType);
                        }
                    }
                    return contentTypes;
                });

        /// <summary>
        /// Converts a <see cref="ReportFormat"/> into a MIME type.
        /// </summary>
        /// <param name="format">The format to convert.</param>
        /// <returns>A string containing the MIME type.</returns>
        public static string FromReportFormat(ReportFormat format)
        {
            return _contentTypes.Value.TryGetValue(format, out var mimeType)
                ? mimeType
                : throw new ApplicationException($"Unknown report format {format}");
        }
    }
}