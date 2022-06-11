namespace NaoBlocks.Client.Terminal
{
    /// <summary>
    /// The standard console.
    /// </summary>
    public class StandardConsole
        : IConsole
    {
        /// <summary>
        /// Writes an error message to the console.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void WriteError(string message)
        {
            WriteMessage(message, ConsoleColor.Red);
        }

        /// <summary>
        /// Writes a message to the console.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void WriteMessage(string message)
        {
            Console.WriteLine(message);
        }

        private static void WriteMessage(string message, ConsoleColor color)
        {
            var currentColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = color;
                Console.WriteLine(message);
            }
            finally
            {
                Console.ForegroundColor = currentColor;
            }
        }
    }
}