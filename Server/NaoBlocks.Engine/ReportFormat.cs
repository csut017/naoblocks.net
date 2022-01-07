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
        Pdf,

        /// <summary>
        /// Generate an Excel report.
        /// </summary>
        Excel,

        /// <summary>
        /// Generate a Zip file containing the report.
        /// </summary>
        Zip,

        /// <summary>
        /// Generate a CSV report.
        /// </summary>
        Csv,

        /// <summary>
        /// Generate a plain-text report.
        /// </summary>
        Text
    }
}