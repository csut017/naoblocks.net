namespace NaoBlocks.Definitions.Tangibles
{
    /// <summary>
    /// Defines a programmable block in the UI.
    /// </summary>
    public class Block
    {
        /// <summary>
        /// Gets or sets the name of the block.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the NaoLang generator code for the block.
        /// </summary>
        /// <remarks>
        /// The generator code is used to convert the tangible to NaoLang. The JavaScript will be directly downloaded into the client UI.
        /// </remarks>
        public string? Generator { get; set; }

        /// <summary>
        /// Gets or sets the JSON definition of the block.
        /// </summary>
        /// <remarks>
        /// The definition contains the JSON definition of the code. This definition is used to configure the block in the editor.
        /// </remarks>
        public string? Definition { get; set; }
    }
}