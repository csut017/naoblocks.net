namespace NaoBlocks.Definitions.Angular
{
    /// <summary>
    /// Defines a toolbar category in the UI for a robot.
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
        /// The generator code is used to convert the Blockly code to NaoLang. The JavaScript will be directly downloaded into the client UI.
        /// </remarks>
        public string? Generator { get; set; }

        /// <summary>
        /// Gets or sets the JSON definition of the block.
        /// </summary>
        /// <remarks>
        /// The definition contains the JSON definition of the code. This definition is used to configure the block in Blockly.
        /// </remarks>
        public string? Definition { get; set; }

        /// <summary>
        /// Gets or sets the AST converter definition for the block.
        /// </summary>
        /// <remarks>
        /// The AST converter definition is used to convert an AST representation into Blockly XML. The JavaScript will be directly downloaded into the client UI.
        /// </remarks>
        public string? AstConverter { get; set; }

        /// <summary>
        /// Gets or sets the AST name for the converter.
        /// </summary>
        /// <remarks>
        /// The AST name is the name of the function returned from the AST parser.
        /// </remarks>
        public string? AstName { get; set; }
    }
}
