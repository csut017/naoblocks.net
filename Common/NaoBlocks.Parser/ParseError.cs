using NaoBlocks.Common;

namespace NaoBlocks.Parser
{
    /// <summary>
    /// An error from parsing a NaoLang program.
    /// </summary>
    public class ParseError
    {
        /// <summary>
        /// Initialises a new <see cref="ParseError"/>.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="token">The token where the error occurred.</param>
        public ParseError(string message, Token token)
        {
            this.Message = message;
            this.LineNumber = token.LineNumber;
            this.LinePosition = token.LinePosition;
        }

        /// <summary>
        /// The line number where the error occurred.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// The line position where the error occurred.
        /// </summary>
        public int LinePosition { get; set; }

        /// <summary>
        /// A message explaining the error.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Generates a string representation of the error.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{this.Message} [{this.LineNumber}:{this.LinePosition}]";
        }
    }
}