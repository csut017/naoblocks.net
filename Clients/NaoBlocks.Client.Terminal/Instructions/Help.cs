namespace NaoBlocks.Client.Terminal.Instructions
{
    /// <summary>
    ///  An instruction to list all the instructions.
    /// </summary>
    [Instruction(InstructionName, "Displays the help information")]
    public class Help
        : InstructionBase
    {
        /// <summary>
        /// The name of this instruction.
        /// </summary>
        public const string InstructionName = "help";

        private InstructionBase? instruction;

        /// <summary>
        /// Retrieves the help text for the instruction.
        /// </summary>
        /// <returns>An enumerable containing one string per line of text.</returns>
        public override IEnumerable<string> RetrieveHelpText()
        {
            return new[]
            {
                "Syntax: help [instruction]",
                string.Empty,
                "Retrieves the help information"
            };
        }

        /// <summary>
        /// Runs the instruction.
        /// </summary>
        /// <param name="console">The console for writing any output.</param>
        /// <returns>The return code from the instruction.</returns>
        public override Task<int> RunAsync(IConsole console)
        {
            if (this.Factory == null)
            {
                throw new InvalidOperationException("Instruction is in an invalid state: this should not happen when it is called from App!");
            }

            if (this.instruction == null)
            {
                foreach (var instruction in this.Factory.List())
                {
                    console.WriteMessage(GenerateNameLine(instruction));
                }

                return Task.FromResult(0);
            }

            console.WriteMessage(GenerateNameLine(instruction));
            foreach (var line in instruction.RetrieveHelpText())
            {
                console.WriteMessage(line ?? String.Empty);
            }
            return Task.FromResult(0);
        }

        /// <summary>
        /// Validates the instruction.
        /// </summary>
        /// <param name="console">The console for writing any output.</param>
        /// <param name="args">The arguments for the instruction.</param>
        /// <returns>True if the instruction is valid; false otherwise.</returns>
        public override bool Validate(IConsole console, string[] args)
        {
            if (this.Factory == null)
            {
                throw new InvalidOperationException("Instruction is in an invalid state: this should not happen when it is called from App!");
            }

            var count = args.Length;
            if (count > 2)
            {
                console.WriteError("Invalid number of arguments. Syntax is help [instruction]");
                return false;
            }

            if (count == 2)
            {
                this.instruction = this.Factory.Retrieve(args[1]);
                if (this.instruction == null)
                {
                    console.WriteError("Unknown instruction 'unknown'");
                    return false;
                }
            }

            return true;
        }

        private static string GenerateNameLine(InstructionBase instruction)
        {
            return string.IsNullOrWhiteSpace(instruction.Description)
                                    ? instruction.Name
                                    : $"{instruction.Name}: {instruction.Description}";
        }
    }
}