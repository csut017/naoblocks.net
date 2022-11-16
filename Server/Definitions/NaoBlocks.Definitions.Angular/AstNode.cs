namespace NaoBlocks.Definitions.Angular
{
    /// <summary>
    /// Defines a converter for an AST node.
    /// </summary>
    public class AstNode
    {
        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the NaoLang converter code for the node.
        /// </summary>
        /// <remarks>
        /// The generator code is used to convert the AST node to Blocky XML. The JavaScript will be directly downloaded into the client UI.
        /// </remarks>
        public string? Converter { get; set; }
    }
}
