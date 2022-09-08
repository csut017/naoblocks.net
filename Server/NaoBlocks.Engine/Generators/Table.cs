using OfficeOpenXml;

namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// An internal data representation of a table.
    /// </summary>
    public class Table
        : ReportItem
    {
        /// <summary>
        /// Gets the rows in the table.
        /// </summary>
        public List<TableRow> Rows { get; private set; } = new List<TableRow>();

        /// <summary>
        /// Adds a row to the table.
        /// </summary>
        /// <param name="type">The type of the row.</param>
        /// <param name="data">The cell values.</param>
        public TableRow AddRow(TableRowType type, params TableCell?[] data)
        {
            var row = new TableRow
            {
                Type = type
            };
            row.Values.AddRange(data);
            this.Rows.Add(row);
            return row;
        }

        /// <summary>
        /// Adds a row to the table.
        /// </summary>
        /// <param name="data">The cell values.</param>
        public TableRow AddRow(params TableCell?[] data)
        {
            return this.AddRow(TableRowType.Data, data);
        }

        /// <summary>
        /// Ensures all rows have the same number of cells in them.
        /// </summary>
        public void EnsureAllRowsSameLength()
        {
            var maxLength = this.Rows.Max(r => r.Values.Count);
            foreach (var row in this.Rows)
            {
                var needed = maxLength - row.Values.Count;
                row.Values.AddRange(new TableCell[needed]);
            }
        }

        /// <summary>
        /// Exports to CSV.
        /// </summary>
        /// <param name="writer">The <see cref="StreamWriter"/> to use.</param>
        /// <param name="separator">The characters to use as a cell seperator.</param>
        public override void ExportToCsv(StreamWriter writer, string separator)
        {
            foreach (var row in this.Rows)
            {
                var line = string.Join(
                    separator,
                    row.Values.Select(v => v?.ToString() ?? string.Empty));
                writer.WriteLine(line);
                writer.Flush();
            }
        }

        /// <summary>
        /// Exports to Excel.
        /// </summary>
        /// <param name="worksheet">The Excel worksheet to use.</param>
        public override void ExportToExcel(ExcelWorksheet worksheet)
        {
            var rowNum = 0;
            foreach (var row in this.Rows)
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
    }
}