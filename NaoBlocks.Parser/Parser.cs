using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NaoBlocks.Parser
{
    public class Parser
    {
        private readonly Scanner scanner;
        private readonly IDictionary<string, CompoundFunction> compoundFunctions = new Dictionary<string, CompoundFunction>
        {
            { "if", new CompoundFunction { Name = "if", Clauses = new[] { "elseif", "else" } } }
        };
        private Token lastSourceID;
        private Token lastToken;
        private bool hasCached;

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
            var tok = this.scanNextToken();
            if (tok.Type == TokenType.EOF)
            {
                result.Errors.Add(new ParseError("Nothing to parse", tok));
                return result;
            }

            for (; tok.Type != TokenType.EOF; tok = this.scanNextToken())
            {
                if (tok.Type != TokenType.Newline)
                {
                    this.unscan();
                    var node = this.parseItem(result);
                    if (node != null) result.Nodes.Add(node);
                }
            }

            return result;
        }

        private AstNode parseItem(ParseResult result)
        {
            var token = this.scanNextToken();
            if (token.Type != TokenType.Identifier)
            {
                result.Errors.Add(new ParseError("Unexpected token", token));
                return new AstNode(AstNodeType.Invalid, token);
            }

            this.unscan();
            var node = this.parseFunction(result, false, out bool isValid);
            if (node.Type == AstNodeType.Invalid) return node;
            if (!this.compoundFunctions.TryGetValue(node.Token.Value, out CompoundFunction function)) return node;

            //    compound := p.makeNode(p.makeToken(GENERATED, function.Name), NODE_COMPOUND)
            //	compound.Children = append(compound.Children, node)

            //    tok := p.scanNextToken()
            //    p.unscan()
            //	for tok.Type == IDENTIFIER && function.Following[tok.Value] {
            //		node, err := p.parseItem()
            //        compound.Children = append(compound.Children, node)
            //		if err != nil {
            //        return compound, err

            //        }
            //    tok = p.scanNextToken()
            //    p.unscan()

            return node;
        }

        private AstNode parseFunction(ParseResult result, bool isArg, out bool isValid)
        {
            var token = this.scanNextToken();
            isValid = false;

            if (token.Type != TokenType.Identifier)
            {
                this.clearToNewLine();
                result.Errors.Add(new ParseError("Unexpected token", token));
                return new AstNode(AstNodeType.Invalid, token);
            }

            var node = new AstNode(AstNodeType.Function, token);
            token = this.scanNextToken();
            isValid = true;
            return node;
        }

        private void clearToNewLine()
        {
            var token = this.scanNextToken();
            while ((token.Type != TokenType.EOF) && (token.Type != TokenType.Newline))
            {
                token = this.scanNextToken();
            }

            this.unscan();
        }

        private Token scanNextToken()
        {
            var token = this.scan();
            while (true)
            {
                if (token.Type == TokenType.SourceID)
                {
                    this.lastSourceID = token;
                }
                else if (token.Type != TokenType.Whitespace)
                {
                    break;
                }

                token = this.scan();
            }

            return token;
        }

        private Token scan()
        {
            if (this.hasCached)
            {
                this.hasCached = false;
                return this.lastToken;
            }

            this.lastToken = this.scanner.Read();
            return this.lastToken;
        }

        private void unscan()
        {
            this.hasCached = true;
        }

        private struct CompoundFunction
        {
            public string Name { get; set; }

            public string[] Clauses { get; set; }
        }
    }
}
