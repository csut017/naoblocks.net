namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// A code program.
    /// </summary>
    public class CodeProgram
    {
        /// <summary>
        /// Gets or sets the code for the program.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the program name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the program number.
        /// </summary>
        public long? Number { get; set; }

        /// <summary>
        /// Gets or sets the source of the code (e.g. Blockly, TopCodes, etc.)
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user this program belongs to.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets when this program was added.
        /// </summary>
        public DateTime WhenAdded { get; set; }
    }
}