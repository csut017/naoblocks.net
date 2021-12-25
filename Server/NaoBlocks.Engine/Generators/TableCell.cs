namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Internal representation of a cell in a table.
    /// </summary>
    internal class TableCell
    {
        /// <summary>
        /// Initialise a new <see cref="TableCell"/> instance.
        /// </summary>
        public TableCell()
        {
        }

        /// <summary>
        /// Initialise a new <see cref="TableCell"/> with a value.
        /// </summary>
        /// <param name="value">The value of the cell.</param>
        /// <param name="format">The format of the cell.</param>
        public TableCell(object? value, string? format = null)
        {
            this.Value = value;
            this.Format = format;
        }

        /// <summary>
        /// Gets or sets the cell format.
        /// </summary>
        public string? Format { get; set; }

        /// <summary>
        /// Gets or sets the cell value.
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Converts from a <see cref="DateTime"/> to a <see cref="TableCell"/>.
        /// </summary>
        /// <param name="value">The <see cref="DateTime"/> to convert.</param>
        public static implicit operator TableCell(DateTime value) => new(value, "d-MMM-yyyy");

        /// <summary>
        /// Converts from a <see cref="string"/> to a <see cref="TableCell"/>.
        /// </summary>
        /// <param name="value">The <see cref="string"/> to convert.</param>
        public static implicit operator TableCell(string? value) => new(value);

        /// <summary>
        /// Converts from a <see cref="bool"/> to a <see cref="TableCell"/>.
        /// </summary>
        /// <param name="value">The <see cref="bool"/> to convert.</param>
        public static implicit operator TableCell(bool? value) => new(value);

        /// <summary>
        /// Converts from a <see cref="int"/> to a <see cref="TableCell"/>.
        /// </summary>
        /// <param name="value">The <see cref="int"/> to convert.</param>
        public static implicit operator TableCell(int? value) => new(value, "0");

        /// <summary>
        /// Converts from a <see cref="long"/> to a <see cref="TableCell"/>.
        /// </summary>
        /// <param name="value">The <see cref="long"/> to convert.</param>
        public static implicit operator TableCell(long? value) => new(value, "0");

        /// <summary>
        /// Converts from a <see cref="double"/> to a <see cref="TableCell"/>.
        /// </summary>
        /// <param name="value">The <see cref="double"/> to convert.</param>
        public static implicit operator TableCell(double? value) => new(value, "0.00");

        /// <summary>
        /// Generates a string version of this <see cref="TableCell"/>.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            if (this.Value == null) return string.Empty;
            if (this.Format != null)
            {
                if (this.Value is DateTime) return ((DateTime)this.Value).ToString(this.Format);
                if (this.Value is long) return ((long)this.Value).ToString(this.Format);
                if (this.Value is int) return ((int)this.Value).ToString(this.Format);
                if (this.Value is double) return ((double)this.Value).ToString(this.Format);
            }

            if (this.Value is bool) return true.Equals(this.Value) ? "Yes" : "No";
            return this.Value.ToString() ?? string.Empty;
        }
    }
}