﻿using System.Runtime.Serialization;

namespace NaoBlocks.Client.Terminal
{
    /// <summary>
    /// An exception that occured in an <see cref="InstructionFactory"/> instance.
    /// </summary>
    public class InstructionFactoryException
        : Exception
    {
        /// <summary>
        /// Initialise a new <see cref="InstructionFactoryException"/> with a message.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public InstructionFactoryException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Initialise a new <see cref="InstructionFactoryException"/> with a message and inner exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public InstructionFactoryException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initialise a <see cref="IndexOutOfRangeException"/> from serialisation.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        protected InstructionFactoryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}