namespace NaoBlocks.Engine
{
    /// <summary>
    /// Specifies the format of an export.
    /// </summary>
    public enum ReportFormat
    {
        /// <summary>
        /// The report format is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// Generate a PDF report.
        /// </summary>
        [ReportFormat("application/pdf", "pdf")]
        Pdf,

        /// <summary>
        /// Generate an Excel report.
        /// </summary>
        [ReportFormat("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx")]
        Excel,

        /// <summary>
        /// Generate a Zip file containing the report.
        /// </summary>
        [ReportFormat("application/zip", "zip")]
        Zip,

        /// <summary>
        /// Generate a CSV report.
        /// </summary>
        [ReportFormat("text/csv", "csv")]
        Csv,

        /// <summary>
        /// Generate a plain-text report.
        /// </summary>
        [ReportFormat("text/plain", "txt")]
        Text,

        /// <summary>
        /// Generate an XML report.
        /// </summary>
        [ReportFormat("application/xml", "xml")]
        Xml,
    }
}