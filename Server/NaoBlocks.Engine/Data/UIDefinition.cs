namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// Holds a User Interface definition
    /// </summary>
    public class UIDefinition
    {
        /// <summary>
        /// Gets or sets the name of the definition.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the definition.
        /// </summary>
        public IUIDefinition? Definition { get; set; }
    }
}
