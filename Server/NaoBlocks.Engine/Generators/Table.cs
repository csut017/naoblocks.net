namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// An internal data representation of a table.
    /// </summary>
    internal class Table
    {
        /// <summary>
        /// Gets or sets the table name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets the rows in the table.
        /// </summary>
        public List<TableRow> Rows { get; private set; } = new List<TableRow>();

        /// <summary>
        /// Adds a row to the table.
        /// </summary>
        /// <param name="type">The type of the row.</param>
        /// <param name="data">The cell values.</param>
        public void AddRow(TableRowType type, params TableCell?[] data)
        {
            var row = new TableRow
            {
                Type = type
            };
            row.Values.AddRange(data);
            this.Rows.Add(row);
        }

        /// <summary>
        /// Adds a row to the table.
        /// </summary>
        /// <param name="data">The cell values.</param>
        public void AddRow(params TableCell?[] data)
        {
            this.AddRow(TableRowType.Data, data);
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
    }
}