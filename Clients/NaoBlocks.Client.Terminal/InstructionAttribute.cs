namespace NaoBlocks.Client.Terminal
{
    /// <summary>
    /// Defines an instruction.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class InstructionAttribute
        : Attribute
    {
        /// <summary>
        /// Defines a new <see cref="InstructionAttribute"/> instance.
        /// </summary>
        /// <param name="name">The name of the instruction.</param>
        /// <param name="description">The description of the instruction.</param>
        public InstructionAttribute(string name, string? description = null)
        {
            this.Name = name;
            Description = description;
        }

        /// <summary>
        /// Gets the description of the instruction.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Gets the name of the instruction.
        /// </summary>
        public string Name { get; }
    }
}