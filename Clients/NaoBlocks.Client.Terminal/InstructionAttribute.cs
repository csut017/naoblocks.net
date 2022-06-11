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
        public InstructionAttribute(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets the name of an instruction.
        /// </summary>
        public string Name { get; }
    }
}