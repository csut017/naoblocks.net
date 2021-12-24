using OfficeOpenXml;
using System.Text;

namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// A generator that will automatically generate documents based on the format.
    /// </summary>
    internal class Generator
    {
        private readonly List<Table> tables = new();

        /// <summary>
        /// Initialise the license for EPPlus.
        /// </summary>
        static Generator()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Starts a new <see cref="Table"/> and adds it.
        /// </summary>
        /// <param name="name">The name of the table.</param>
        /// <returns>The add <see cref="Table"/>.</returns>
        public Table AddTable(string name)
        {
            var table = new Table { Name = name };
            this.tables.Add(table);
            return table;
        }

        /// <summary>
        /// Generates the document.
        /// </summary>
        /// <param name="format">The format to generate.</param>
        /// <param name="baseName">The base name of the file.</param>
        /// <returns>A <see cref="Stream"/> containing the data and the filename.</returns>
        public async Task<(Stream, string)> GenerateAsync(ReportFormat format, string baseName)
        {
            return format switch
            {
                ReportFormat.Excel => await this.GenerateExcel(baseName),
                ReportFormat.Csv => this.GenerateText(baseName, "csv", false, ","),
                ReportFormat.Text => this.GenerateText(baseName, "txt", true, ","),
                _ => throw new ApplicationException($"Unable to generate: unhandled {format}"),
            };
        }

        private async Task<(Stream, string)> GenerateExcel(string baseName)
        {
            using var package = new ExcelPackage();
            var tableNum = 0;
            foreach (var table in this.tables)
            {
                tableNum++;
                var tableName = string.IsNullOrWhiteSpace(table.Name)
                    ? $"Table {tableNum}"
                    : table.Name;
                var worksheet = package.Workbook.Worksheets.Add(tableName);
                var rowNum = 0;
                foreach (var row in table.Rows)
                {
                    rowNum++;
                    var cellNum = 0;
                    foreach (var cell in row.Values)
                    {
                        cellNum++;
                        if (cell == null) continue;

                        var destCell = worksheet.Cells[rowNum, cellNum];
                        destCell.Value = cell.Value;
                        var style = destCell.Style;
                        if (cell.Format != null) style.Numberformat.Format = cell.Format;
                        if (row.Type == TableRowType.Header) style.Font.Bold = true;
                    }
                }
            }

            var stream = new MemoryStream();
            await package.SaveAsAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return (stream, $"{baseName}.xlsx");
        }

        private (Stream, string) GenerateText(string baseName, string extension, bool includeHeader, string separator)
        {
            var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true);

            var tableNum = 0;
            foreach (var table in this.tables)
            {
                tableNum++;
                if (includeHeader)
                {
                    var tableName = string.IsNullOrWhiteSpace(table.Name)
                        ? $"Table {tableNum}"
                        : table.Name;
                    writer.WriteLine(tableName);
                    writer.WriteLine(new string('=', tableName.Length));
                }

                foreach (var row in table.Rows)
                {
                    var line = string.Join(
                        separator,
                        row.Values.Select(v => v?.ToString() ?? string.Empty));
                    writer.WriteLine(line);
                    writer.Flush();
                }
            }

            stream.Seek(0, SeekOrigin.Begin);
            return (stream, $"{baseName}.{extension}");
        }
    }
}