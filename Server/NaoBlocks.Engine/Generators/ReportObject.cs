using OfficeOpenXml;

namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Defines an object that can go in a report.
    /// </summary>
    public abstract class ReportItem
    {
        /// <summary>
        /// Gets or sets the object name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Exports to CSV.
        /// </summary>
        /// <param name="writer">The <see cref="StreamWriter"/> to use.</param>
        /// <param name="separator">The characters to use as a cell seperator.</param>
        public abstract void ExportToCsv(StreamWriter writer, string separator);

        /// <summary>
        /// Exports to Excel.
        /// </summary>
        /// <param name="worksheet">The Excel worksheet to use.</param>
        public abstract void ExportToExcel(ExcelWorksheet worksheet);
    }
}