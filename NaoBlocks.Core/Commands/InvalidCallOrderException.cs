using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace NaoBlocks.Core.Commands
{
    public class InvalidCallOrderException : Exception
    {
        public InvalidCallOrderException(string message)
            : base(message)
        {
        }

        public InvalidCallOrderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Standard error message")]
        public InvalidCallOrderException()
            : base("Invalid call order")
        {
        }

        protected InvalidCallOrderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}