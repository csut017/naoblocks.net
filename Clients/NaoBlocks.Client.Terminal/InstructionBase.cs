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
        /// Displays the help text.
        /// </summary>
        /// <param name="console">The console to write to.</param>
        public virtual void DisplayHelpText(IConsole console)
        {
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

        /// <summary>
        /// Parses the args and removes any named arguments.
        /// </summary>
        /// <param name="args">The args to parse.</param>
        /// <returns>The first return value is the non-named args, the second is a dictionary with the named args.</returns>
        protected static (string[], IDictionary<string, string[]>) ParseNamedArgs(string[] args)
        {
            var outputArgs = new List<string>();
            var namedArgs = new Dictionary<string, List<string>>();
            for (var loop = 0; loop < args.Length; loop++)
            {
                var arg = args[loop];
                if (arg.StartsWith("--"))
                {
                    var pos = arg.IndexOf('=');
                    var name = arg[2..];
                    var value = string.Empty;
                    if (pos > 0)
                    {
                        name = arg[2..(pos - 1)];
                        value = arg[pos..];
                    }

                    if (namedArgs.TryGetValue(name, out var existing))
                    {
                        existing.Add(value);
                    }
                    else
                    {
                        namedArgs.Add(name, new List<string> { value });
                    }
                }
                else
                {
                    outputArgs.Add(arg);
                }
            }

            return (
                outputArgs.ToArray(),
                namedArgs.ToDictionary(
                    i => i.Key,
                    i => i.Value.ToArray())
            );
        }

        /// <summary>
        /// Checks that factory has been set.
        /// </summary>
        protected void CheckFactoryIsSet()
        {
            if (this.Factory == null)
            {
                throw new InvalidOperationException("Instruction is in an invalid state: this should not happen when it is called from App!");
            }
        }
    }
}