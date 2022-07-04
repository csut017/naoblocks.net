namespace NaoBlocks.Definitions.Angular
{
    /// <summary>
    /// Defines a programmable block in the UI.
    /// </summary>
    public class Block
    {
        /// <summary>
        /// Gets or sets the category of the block.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Gets or sets the JSON definition of the block.
        /// </summary>
        /// <remarks>
        /// The definition contains the JSON definition of the code. This definition is used to configure the block in Blockly.
        /// </remarks>
        public string? Definition { get; set; }

        /// <summary>
        /// Gets or sets the NaoLang generator code for the block.
        /// </summary>
        /// <remarks>
        /// The generator code is used to convert the Blockly code to NaoLang. The JavaScript will be directly downloaded into the client UI.
        /// </remarks>
        public string? Generator { get; set; }

        /// <summary>
        /// Gets or sets the name of the block.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the text title of the block.
        /// </summary>
        public string? Text { get; set; }
    }
}