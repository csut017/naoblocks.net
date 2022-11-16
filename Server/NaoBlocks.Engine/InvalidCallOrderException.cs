using System.Runtime.Serialization;

namespace NaoBlocks.Engine
{
    /// <summary>
    /// An exception that occurs when a command is called out of order.
    /// </summary>
    public class InvalidCallOrderException : Exception
    {
        private const string defaultMessage = "Invalid call order";

        /// <summary>
        /// Initialise a <see cref="InvalidCallOrderException"/> instance with the default message.
        /// </summary>
        public InvalidCallOrderException()
            : base(defaultMessage)
        {
        }

        /// <summary>
        /// Initialise a <see cref="InvalidCallOrderException"/> instance with a message.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public InvalidCallOrderException(string? message) 
            : base(message ?? defaultMessage)
        {
        }

        /// <summary>
        /// Initialise a <see cref="InvalidCallOrderException"/> instance with a message and inner exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public InvalidCallOrderException(string? message, Exception? innerException) 
            : base(message ?? defaultMessage, innerException)
        {
        }
    }
}