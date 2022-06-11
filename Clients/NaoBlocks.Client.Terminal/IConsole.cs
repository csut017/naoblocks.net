namespace NaoBlocks.Client.Terminal
{
    /// <summary>
    /// Defines a console that can be used by instructions.
    /// </summary>
    public interface IConsole
    {
        /// <summary>
        /// Writes an error message to the console.
        /// </summary>
        /// <param name="message">The message to write.</param>
        void WriteError(string message);

        /// <summary>
        /// Writes a message to the console.
        /// </summary>
        /// <param name="message">The message to write.</param>
        void WriteMessage(string message);
    }
}