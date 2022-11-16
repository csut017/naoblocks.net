namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// A named value.
    /// </summary>
    public class NamedValue
    {
        /// <summary>
        /// The name of the value.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The value.
        /// </summary>
        public string? Value { get; set; }
    }
}