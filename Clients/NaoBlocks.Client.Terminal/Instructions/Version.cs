namespace NaoBlocks.Client.Terminal.Instructions
{
    /// <summary>
    /// An instruction for retrieving the server version.
    /// </summary>
    [Instruction("version", "Retrieves the server version")]
    public class Version
        : ServerInstructionBase
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
        public override async Task<int> RunAsync(IConsole console)
        {
            this.CheckFactoryIsSet();
            var connection = this.Factory!.RetrieveConnection();
            var version = await connection.RetrieveServerVersion();
            console.WriteMessage($"The current version is {version.Version}");
            return 0;
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
            var (posArgs, _) = ParseNamedArgs(args);
            if (posArgs.Length != 2)
            {
                WriteErrorMessage(console, "Invalid number of arguments.");
                return false;
            }

            this.Factory!.ServerAddress = posArgs[1];
            if (!this.Factory.CheckIfAddressIsValid())
            {
                WriteErrorMessage(console, "Invalid server address - must start with https:// or http://");
                return false;
            }

            return true;
        }

        private static void WriteErrorMessage(IConsole console, params string[] messages)
        {
            foreach (var message in messages)
            {
                console.WriteError(message);
            }

            console.WriteMessage();
            console.WriteMessage($"Usage is {App.ApplicationName} version <SERVER> [options]");
        }
    }
}