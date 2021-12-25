namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// An internal data representation of a row in a table.
    /// </summary>
    internal class TableRow
    {
        /// <summary>
        /// Gets or sets the row type.
        /// </summary>
        public TableRowType Type { get; set; } = TableRowType.Data;

        /// <summary>
        /// Gets or sets the values.
        /// </summary>
        public List<TableCell?> Values { get; private set; } = new List<TableCell?>();

        /// <summary>
        /// Sets a cell value, ensuring any extra cells are added.
        /// </summary>
        /// <param name="column">The column number to set.</param>
        /// <param name="value">The cell to set.</param>
        /// <returns>The <see cref="TableCell"/> that was set.</returns>
        public TableCell SetCell(int column, TableCell value)
        {
            for (var loop = this.Values.Count; loop <= column; loop++)
            {
                this.Values.Add(null);
            }
            this.Values[column] = value;
            return value;
        }
    }
}