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
        /// Writes one or more messages to the console.
        /// </summary>
        /// <param name="messages">The messages to write.</param>
        public void WriteMessage(params string[] messages)
        {
            if (messages.Length == 0)
            {
                Console.WriteLine();
            }

            foreach (var message in messages)
            {
                Console.WriteLine(message);
            }
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