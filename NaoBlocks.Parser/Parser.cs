using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NaoBlocks.Parser
{
    public class Parser
    {
        private readonly Scanner scanner;

        public Parser(Scanner scanner)
        {
            this.scanner = scanner;
        }

        public static Parser New(string codeToParse)
        {
            var input = new StringReader(codeToParse);
            var scanner = new Scanner(input);
            return new Parser(scanner);
        }

        public ParseResult Parse()
        {
            var result = new ParseResult();

            var tok = this.scanner.Read();
            if (tok.Type == TokenType.EOF)
            {
                result.Errors.Add(new ParseError("Nothing to parse", tok));
            }

            return result;
        }
    }
}
