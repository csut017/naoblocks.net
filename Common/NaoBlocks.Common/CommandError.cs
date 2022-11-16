namespace NaoBlocks.Common
{
    /// <summary>
    /// An error that has occurred while executing a command.
    /// </summary>
    public class CommandError
    {
        /// <summary>
        /// Initialises a new <see cref="CommandError"/> instance.
        /// </summary>
        /// <param name="number">The command number.</param>
        /// <param name="error">The error messages.</param>
        public CommandError(int number, string error)
        {
            this.Number = number;
            this.Error = error;
        }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Gets the command number that caused the error.
        /// </summary>
        public int Number { get; private set; }
    }
}