using OfficeOpenXml;
using System.Text;

namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// A generator that will automatically generate documents based on the format.
    /// </summary>
    public class Generator
    {
        private readonly List<ReportItem> items = new();

        /// <summary>
        /// Initialise the license for EPPlus.
        /// </summary>
        static Generator()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Starts a new page.
        /// </summary>
        /// <param name="name">The name of the page.</param>
        /// <returns>A new <see cref="Page"/> instance.</returns>
        public Page AddPage(string name)
        {
            var page = new Page { Name = name };
            this.items.Add(page);
            return page;
        }

        /// <summary>
        /// Starts a new <see cref="Table"/> and adds it.
        /// </summary>
        /// <param name="name">The name of the table.</param>
        /// <returns>The add <see cref="Table"/>.</returns>
        public Table AddTable(string name)
        {
            var table = new Table { Name = name };
            this.items.Add(table);
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
                ReportFormat.Csv => this.GenerateText(baseName, "csv", false, ",", ","),
                ReportFormat.Text => this.GenerateText(baseName, "txt", true, ",", ": "),
                _ => throw new ApplicationException($"Unable to generate: unhandled {format}"),
            };
        }

        private async Task<(Stream, string)> GenerateExcel(string baseName)
        {
            using var package = new ExcelPackage();
            var itemNum = 0;
            foreach (var item in this.items)
            {
                itemNum++;
                var itemName = string.IsNullOrWhiteSpace(item.Name)
                    ? $"Table {itemNum}"
                    : item.Name;
                var worksheet = package.Workbook.Worksheets.Add(itemName);
                item.ExportToExcel(worksheet);
            }

            var stream = new MemoryStream();
            await package.SaveAsAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return (stream, $"{baseName}.xlsx");
        }

        private (Stream, string) GenerateText(string baseName, string extension, bool includeHeader, string tableSeparator, string pageSeperator)
        {
            var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true);

            var itemNum = 0;
            foreach (var item in this.items)
            {
                itemNum++;
                if (includeHeader)
                {
                    var tableName = string.IsNullOrWhiteSpace(item.Name)
                        ? $"Table {itemNum}"
                        : item.Name;
                    writer.WriteLine(tableName);
                    writer.WriteLine(new string('=', tableName.Length));
                }

                var separator = string.Empty;
                switch (item)
                {
                    case Table:
                        separator = tableSeparator;
                        break;

                    case Page _:
                        separator = pageSeperator;
                        break;
                }
                item.ExportToCsv(writer, separator);
            }

            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            return (stream, $"{baseName}.{extension}");
        }
    }
}