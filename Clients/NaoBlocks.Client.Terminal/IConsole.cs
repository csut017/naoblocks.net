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
        /// Writes one or more messages to the console.
        /// </summary>
        /// <param name="messages">The messages to write.</param>
        void WriteMessage(params string[] messages);
    }
}