namespace NaoBlocks.Client.Terminal
{
    /// <summary>
    /// An exception that occurred in the connection.
    /// </summary>
    public class ConnectionException
        : Exception
    {
        /// <summary>
        /// Initialises a new instance of <see cref="ConnectionException"/>.
        /// </summary>
        /// <param name="message">The error message.</param>
        public ConnectionException(string message)
            : base(message)
        {
        }
    }
}