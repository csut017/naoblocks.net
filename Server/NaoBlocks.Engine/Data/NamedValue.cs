using System.Diagnostics;

namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// A named value.
    /// </summary>
    [DebuggerDisplay("{Name}->{Value}")]
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

        /// <summary>
        /// Generates a new <see cref="NamedValue"/> instance.
        /// </summary>
        /// <param name="name">The name of the value.</param>
        /// <param name="value">The value.</param>
        /// <returns>The new <see cref="NamedValue"/> instance.</returns>
        public static NamedValue New(string name, string value)
        {
            return new NamedValue { Name = name, Value = value };
        }
    }
}