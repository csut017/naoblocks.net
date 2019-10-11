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
            { "if", new CompoundFunction { Name = "if", Clauses = new HashSet<string>(new[] { "elseif", "else" }) } }
        };
        private readonly IDictionary<TokenType, Func<ParseResult, ParseOperationResult>> parseFunctions;
        private Token lastSourceID;
        private Token lastToken;
        private bool hasCached;

        public Parser(Scanner scanner)
        {
            this.scanner = scanner;
            parseFunctions = new Dictionary<TokenType, Func<ParseResult, ParseOperationResult>>
                {
                    { TokenType.Boolean, this.parseConstant },
                    { TokenType.Colour, this.parseConstant },
                    { TokenType.Constant, this.parseConstant },
                    { TokenType.Identifier, this.parseFunctionAsArg },
                    { TokenType.Number, this.parseConstant },
                    { TokenType.Text, this.parseConstant },
                };
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
                    var res = this.parseItem(result);
                    if (res.Node != null) result.Nodes.Add(res.Node);
                }
            }

            return result;
        }

        private ParseOperationResult parseItem(ParseResult result)
        {
            var token = this.scanNextToken();
            if (token.Type != TokenType.Identifier)
            {
                result.Errors.Add(new ParseError("Unexpected token", token));
                return new ParseOperationResult(new AstNode(AstNodeType.Invalid, token));
            }

            this.unscan();
            var parseResult = this.parseFunction(result, false);
            if (!parseResult.IsValid) return new ParseOperationResult(parseResult.Node);
            if (!this.compoundFunctions.TryGetValue(parseResult.Node.Token.Value, out CompoundFunction function))
            {
                return new ParseOperationResult(parseResult.Node, true);
            }

            var compound = new AstNode(AstNodeType.Compound, new Token(TokenType.Generated, function.Name));
            compound.Children.Add(parseResult.Node);
            token = this.scanNextToken();
            this.unscan();
            while ((token.Type == TokenType.Identifier) && function.Clauses.Contains(token.Value))
            {
                parseResult = this.parseFunction(result, false);
                compound.Children.Add(parseResult.Node);
                if (parseResult.IsValid) return new ParseOperationResult(compound);

                token = this.scanNextToken();
                this.unscan();
            }

            return new ParseOperationResult(compound, true);
        }

        private ParseOperationResult parseFunction(ParseResult result, bool isArg)
        {
            var token = this.scanNextToken();

            if (token.Type != TokenType.Identifier)
            {
                this.clearToNewLine();
                result.Errors.Add(new ParseError("Unexpected token", token));
                return new ParseOperationResult(new AstNode(AstNodeType.Invalid, token));
            }

            var node = new AstNode(AstNodeType.Function, token);
            token = this.scanNextToken();
            if (!((token.Type == TokenType.OpenBracket) || (token.Type == TokenType.OpenBrace)))
            {
                this.clearToNewLine();
                result.Errors.Add(new ParseError("Unexpected token", token));
                return new ParseOperationResult(node);
            }

            if (token.Type == TokenType.OpenBracket)
            {
                token = this.scanNextToken();
                while (token.Type != TokenType.CloseBracket)
                {
                    this.unscan();
                    var argument = this.parseFunctionArg(result);
                    if (!argument.IsValid) return new ParseOperationResult(node);
                    if (argument.Node != null) node.Arguments.Add(argument.Node);

                    token = this.scanNextToken();
                    if (token.Type == TokenType.Comma) token = this.scanNextToken();
                }

                token = this.scanNextToken();
            }

            if (token.Type == TokenType.OpenBrace)
            {
                token = this.scanNextToken();
                if (token.Type != TokenType.Newline)
                {
                    this.clearToNewLine();
                    result.Errors.Add(new ParseError("Expected end of line", token));
                    return new ParseOperationResult(node);
                }

                token = this.scanNextToken();
                while (token.Type != TokenType.CloseBrace)
                {
                    if (token.Type != TokenType.Newline)
                    {
                        this.unscan();
                        var child = this.parseItem(result);
                        if (!child.IsValid) return new ParseOperationResult(node);
                        node.Children.Add(child.Node);
                        token = this.scanNextToken();
                    }

                }
            }
            else
            {
                this.unscan();
            }

            if (!isArg)
            {
                token = this.scanNextToken();
                if (!((token.Type == TokenType.Newline) || (token.Type == TokenType.EOF)))
                {
                    result.Errors.Add(new ParseError("Expected end of line or file", token));
                    return new ParseOperationResult(node);
                }
            }

            return new ParseOperationResult(node, true);
        }

        private ParseOperationResult parseFunctionArg(ParseResult result)
        {
            var token = this.scanNextToken();
            if (!this.parseFunctions.TryGetValue(token.Type, out Func<ParseResult, ParseOperationResult> parseFunction))
            {
                result.Errors.Add(new ParseError("Unable to parse function arg " + token.Type.ToString(), token));
                return new ParseOperationResult(null);
            }

            this.unscan();
            var child = parseFunction(result);
            if (!child.IsValid) return new ParseOperationResult(null);
            return new ParseOperationResult(child.Node, true);
        }

        private ParseOperationResult parseFunctionAsArg(ParseResult result)
        {
            return this.parseFunction(result, true);
        }

        private ParseOperationResult parseConstant(ParseResult result)
        {
            var token = this.scanNextToken();
            return new ParseOperationResult(new AstNode(AstNodeType.Constant, token), true);
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

        private class ParseOperationResult
        {
            public ParseOperationResult(AstNode node, bool isValid = false)
            {
                this.Node = node;
                this.IsValid = isValid;
            }

            public AstNode Node { get; private set; }

            public bool IsValid { get; private set; }
        }

        private struct CompoundFunction
        {
            public string Name { get; set; }

            public HashSet<string> Clauses { get; set; }
        }
    }
}
