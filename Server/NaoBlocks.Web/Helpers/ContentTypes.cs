using NaoBlocks.Engine;

namespace NaoBlocks.Web.Helpers
{
    /// <summary>
    /// Defines some common MIME types.
    /// </summary>
    public static class ContentTypes
    {
        /// <summary>
        /// The file type for a PDF document.
        /// </summary>
        public const string Pdf = "application/pdf";

        /// <summary>
        /// The file type for a png image.
        /// </summary>
        public const string Png = "image/png";

        /// <summary>
        /// The file type for a plain text file.
        /// </summary>
        public const string Txt = "text/plain";

        /// <summary>
        /// The file type for an Excel spreadsheet.
        /// </summary>
        public const string Xlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        /// <summary>
        /// The file type for an XML file.
        /// </summary>
        public const string Xml = "application/xml";

        /// <summary>
        /// The file type for a Zip file.
        /// </summary>
        public const string Zip = "application/zip";

        /// <summary>
        /// Converts a <see cref="ReportFormat"/> into a MIME type.
        /// </summary>
        /// <param name="format">The format to convert.</param>
        /// <returns>A string containing the MIME type.</returns>
        public static string Convert(ReportFormat format)
        {
            return format switch
            {
                ReportFormat.Excel => Xlsx,
                ReportFormat.Pdf => Pdf,
                ReportFormat.Xml => Xml,
                ReportFormat.Zip => Zip,
                _ => throw new ApplicationException($"Unknown report format {format}"),
            };
        }
    }
}