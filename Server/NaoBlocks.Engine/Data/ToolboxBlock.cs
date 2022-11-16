namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// Defines a toolbox block (command).
    /// </summary>
    public class ToolboxBlock
    {
        /// <summary>
        /// Gets or sets the definition of the block.
        /// </summary>
        public string Definition { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the block.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}