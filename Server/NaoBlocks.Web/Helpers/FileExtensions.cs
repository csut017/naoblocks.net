using NaoBlocks.Engine;

namespace NaoBlocks.Web.Helpers
{
    /// <summary>
    /// Defines some common file extensions.
    /// </summary>
    public static class FileExtensions
    {
        private static readonly Lazy<Dictionary<ReportFormat, string>> _fromReportFormat = new(() =>
        {
            var contentTypes = new Dictionary<ReportFormat, string>();
            var fields = typeof(ReportFormat).GetFields();
            foreach (var field in fields)
            {
                if (field.GetCustomAttributes(typeof(ReportFormatAttribute), false).FirstOrDefault() is ReportFormatAttribute attrib)
                {
                    var key = (ReportFormat)field.GetValue(null)!;
                    contentTypes.Add(key, attrib.Extension);
                }
            }
            return contentTypes;
        });

        private static readonly Lazy<Dictionary<string, ReportFormat>> _toReportFormat = new(() =>
        {
            var contentTypes = new Dictionary<string, ReportFormat>();
            var fields = typeof(ReportFormat).GetFields();
            foreach (var field in fields)
            {
                if (field.GetCustomAttributes(typeof(ReportFormatAttribute), false).FirstOrDefault() is ReportFormatAttribute attrib)
                {
                    var key = (ReportFormat)field.GetValue(null)!;
                    contentTypes.Add(attrib.Extension.ToLowerInvariant(), key);
                }
            }
            return contentTypes;
        });

        /// <summary>
        /// Converts a <see cref="ReportFormat"/> into a file extension.
        /// </summary>
        /// <param name="format">The format to convert.</param>
        /// <returns>A string containing the file extension.</returns>
        /// <exception cref="ApplicationException">Thrown if the report format is unknown.</exception>
        public static string FromReportFormat(ReportFormat format)
        {
            return _fromReportFormat.Value.TryGetValue(format, out var extension)
                ? extension
                : throw new ApplicationException($"Unknown report format {format}");
        }

        /// <summary>
        /// Converts a <see cref="ReportFormat"/> from a file extension.
        /// </summary>
        /// <param name="extension">The format to convert.</param>
        /// <returns>The <see cref="ReportFormat"/> if found.</returns>
        /// <exception cref="ApplicationException">Thrown if the file extension is unknown.</exception>
        public static ReportFormat ToReportFormat(string extension)
        {
            if (extension.StartsWith('.')) extension = extension[1..];
            extension = extension.ToLowerInvariant();
            return _toReportFormat.Value.TryGetValue(extension, out var format)
                ? format
                : throw new ApplicationException($"Unknown file extension {extension}");
        }
    }
}