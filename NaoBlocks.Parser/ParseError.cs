using NaoBlocks.Common;

namespace NaoBlocks.Parser
{
    public class ParseError
    {
        public ParseError(string message, Token token)
        {
            this.Message = message;
            this.LineNumber = token.LineNumber;
            this.LinePosition = token.LinePosition;
        }

        public int LineNumber { get; set; }

        public int LinePosition { get; set; }

        public string Message { get; set; }
    }
}