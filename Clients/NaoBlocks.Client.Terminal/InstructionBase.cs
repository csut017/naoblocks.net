namespace NaoBlocks.Client.Terminal
{
    /// <summary>
    /// The base class for all user instructions.
    /// </summary>
    public abstract class InstructionBase
    {
        /// <summary>
        /// Gets or sets the description of the command.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the instruction factory.
        /// </summary>
        public InstructionFactory? Factory { get; set; }

        /// <summary>
        /// Gets or sets the name of the command.
        /// </summary>
        public string Name { get; internal set; } = string.Empty;

        /// <summary>
        /// Retrieves the help text for the instruction.
        /// </summary>
        /// <returns>An enumerable containing one string per line of text.</returns>
        public virtual IEnumerable<string> RetrieveHelpText()
        {
            return Array.Empty<string>();
        }

        /// <summary>
        /// Runs the instruction.
        /// </summary>
        /// <param name="console">The console for writing any output.</param>
        /// <returns>The return code from the instruction.</returns>
        public abstract Task<int> RunAsync(IConsole console);

        /// <summary>
        /// Validates the instruction.
        /// </summary>
        /// <param name="console">The console for writing any output.</param>
        /// <param name="args">The arguments for the instruction.</param>
        /// <returns>True if the instruction is valid; false otherwise.</returns>
        public abstract bool Validate(IConsole console, string[] args);
    }
}