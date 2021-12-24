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
    }
}