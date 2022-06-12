namespace NaoBlocks.Client.Terminal.Instructions
{
    /// <summary>
    /// An instruction for retrieving the server version.
    /// </summary>
    [Instruction("version", "Retrieves the server version")]
    public class Version
        : InstructionBase
    {
        /// <summary>
        /// Displays the help text.
        /// </summary>
        /// <param name="console">The console to write to.</param>
        public override void DisplayHelpText(IConsole console)
        {
            console.WriteMessage(
                $"Usage: {App.ApplicationName} version <SERVER>",
                string.Empty,
                "Arguments:",
                "\t<SERVER>\tThe address of the server to query");
        }

        /// <summary>
        /// Runs the instruction.
        /// </summary>
        /// <param name="console">The console for writing any output.</param>
        /// <returns>The return code from the instruction.</returns>
        public override Task<int> RunAsync(IConsole console)
        {
            this.CheckFactoryIsSet();
            throw new NotImplementedException();
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
            var (posArgs, namedArgs) = ParseNamedArgs(args);
            if (posArgs.Length != 2)
            {
                console.WriteError("Invalid number of arguments.");
                console.WriteMessage();
                console.WriteMessage($"Usage is {App.ApplicationName} version <SERVER> [options]");
                return false;
            }

            return true;
        }
    }
}