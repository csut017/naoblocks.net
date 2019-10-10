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

        public ParseError()
        {
        }

        public string Message { get; set; }

        public int LineNumber { get; set; }

        public int LinePosition { get; set; }
    }
}