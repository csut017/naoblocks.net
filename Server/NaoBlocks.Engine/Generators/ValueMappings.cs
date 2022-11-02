namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Provides various value mappings for help in the generators.
    /// </summary>
    public static class ValueMappings
    {
        /// <summary>
        /// Gets the value mappings for the allocation modes.
        /// </summary>
        public static readonly IReadOnlyDictionary<int, string> AllocationModes = new Dictionary<int, string>
        {
            { 0, "Any" },
            { 1, "Require" },
            { 2, "Prefer" },
        };

        /// <summary>
        /// Gets the value mappings for the view modes.
        /// </summary>
        public static readonly IReadOnlyDictionary<int, string> ViewModes = new Dictionary<int, string>
        {
            { 0, "Blocks" },
            { 1, "Tangibles" },
            { 2, "Role Home" },
        };
    }
}