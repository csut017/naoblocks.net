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
        /// Displays the help text.
        /// </summary>
        /// <param name="console">The console to write to.</param>
        public override void DisplayHelpText(IConsole console)
        {
            console.WriteMessage(
                $"Usage: {App.ApplicationName} help [instruction]",
                string.Empty,
                "Options:",
                "\t[instruction]\tThe name of the instruction to display the help information");
        }

        /// <summary>
        /// Runs the instruction.
        /// </summary>
        /// <param name="console">The console for writing any output.</param>
        /// <returns>The return code from the instruction.</returns>
        public override Task<int> RunAsync(IConsole console)
        {
            this.CheckFactoryIsSet();
            console.WriteMessage();
            if (this.instruction == null)
            {
                console.WriteMessage($"Usage: {App.ApplicationName} [instruction]");
                console.WriteMessage();
                console.WriteMessage("Instructions:");
                foreach (var instruction in this.Factory!.List())
                {
                    console.WriteMessage("\t" + GenerateNameLine(instruction));
                }

                console.WriteMessage();
                console.WriteMessage($"Use {App.ApplicationName} help [instruction] to display the options for an instruction");

                return Task.FromResult(0);
            }

            console.WriteMessage(GenerateNameLine(instruction));
            console.WriteMessage();
            instruction.DisplayHelpText(console);
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
            this.CheckFactoryIsSet();
            var count = args.Length;
            if (count > 2)
            {
                console.WriteError("Invalid number of arguments.");
                console.WriteMessage();
                console.WriteMessage($"Usage is {App.ApplicationName} help [instruction]");
                return false;
            }

            if (count == 2)
            {
                this.instruction = this.Factory!.Retrieve(args[1]);
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