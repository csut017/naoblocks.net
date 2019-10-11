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
        private Token lastSourceId;
        private Token lastToken;
        private bool hasCached;

        public Parser(Scanner scanner)
        {
            this.scanner = scanner;
            parseFunctions = new Dictionary<TokenType, Func<ParseResult, ParseOperationResult>>
                {
                    { TokenType.Boolean, this.ParseConstant },
                    { TokenType.Colour, this.ParseConstant },
                    { TokenType.Constant, this.ParseConstant },
                    { TokenType.Identifier, this.ParseFunction },
                    { TokenType.Number, this.ParseConstant },
                    { TokenType.Text, this.ParseConstant },
                    { TokenType.Variable, this.ParseVariable },
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
            var tok = this.ScanNextToken();
            if (tok.Type == TokenType.EOF)
            {
                result.Errors.Add(new ParseError("Nothing to parse", tok));
                return result;
            }

            for (; tok.Type != TokenType.EOF; tok = this.ScanNextToken())
            {
                if (tok.Type != TokenType.Newline)
                {
                    this.UnscanToken();
                    var res = this.ParseItem(result);
                    if (res.Node != null) result.Nodes.Add(res.Node);
                }
            }

            return result;
        }

        private ParseOperationResult ParseItem(ParseResult result)
        {
            var token = this.ScanNextToken();
            if (token.Type != TokenType.Identifier)
            {
                result.Errors.Add(new ParseError("Unexpected token", token));
                return new ParseOperationResult(this.MakeAstNode(AstNodeType.Invalid, token));
            }

            this.UnscanToken();
            var parseResult = this.ParseFunctionItem(result, false);
            if (!parseResult.IsValid) return new ParseOperationResult(parseResult.Node);
            if (!this.compoundFunctions.TryGetValue(parseResult.Node.Token.Value, out CompoundFunction function))
            {
                return new ParseOperationResult(parseResult.Node, true);
            }

            var compound = this.MakeAstNode(AstNodeType.Compound, new Token(TokenType.Generated, function.Name));
            compound.Children.Add(parseResult.Node);
            token = this.ScanNextToken();
            this.UnscanToken();
            while ((token.Type == TokenType.Identifier) && function.Clauses.Contains(token.Value))
            {
                parseResult = this.ParseFunctionItem(result, false);
                compound.Children.Add(parseResult.Node);
                if (parseResult.IsValid) return new ParseOperationResult(compound);

                token = this.ScanNextToken();
                this.UnscanToken();
            }

            return new ParseOperationResult(compound, true);
        }

        private ParseOperationResult ParseFunctionItem(ParseResult result, bool isArg)
        {
            var token = this.ScanNextToken();

            if (token.Type != TokenType.Identifier)
            {
                this.ClearToNewLine();
                result.Errors.Add(new ParseError("Unexpected token", token));
                return new ParseOperationResult(this.MakeAstNode(AstNodeType.Invalid, token));
            }

            var node = this.MakeAstNode(AstNodeType.Function, token);
            token = this.ScanNextToken();
            if (!((token.Type == TokenType.OpenBracket) || (token.Type == TokenType.OpenBrace)))
            {
                this.ClearToNewLine();
                result.Errors.Add(new ParseError("Unexpected token", token));
                return new ParseOperationResult(node);
            }

            if (token.Type == TokenType.OpenBracket)
            {
                token = this.ScanNextToken();
                while (token.Type != TokenType.CloseBracket)
                {
                    this.UnscanToken();
                    var argument = this.ParseFunctionArgument(result);
                    if (!argument.IsValid) return new ParseOperationResult(node);
                    if (argument.Node != null) node.Arguments.Add(argument.Node);

                    token = this.ScanNextToken();
                    if (token.Type == TokenType.Comma) token = this.ScanNextToken();
                }

                token = this.ScanNextToken();
            }

            if (token.Type == TokenType.OpenBrace)
            {
                token = this.ScanNextToken();
                if (token.Type != TokenType.Newline)
                {
                    this.ClearToNewLine();
                    result.Errors.Add(new ParseError("Expected end of line", token));
                    return new ParseOperationResult(node);
                }

                token = this.ScanNextToken();
                while (token.Type != TokenType.CloseBrace)
                {
                    if (token.Type != TokenType.Newline)
                    {
                        this.UnscanToken();
                        var child = this.ParseItem(result);
                        if (!child.IsValid) return new ParseOperationResult(node);
                        node.Children.Add(child.Node);
                        token = this.ScanNextToken();
                    }

                }
            }
            else
            {
                this.UnscanToken();
            }

            if (!isArg)
            {
                token = this.ScanNextToken();
                if (!((token.Type == TokenType.Newline) || (token.Type == TokenType.EOF)))
                {
                    result.Errors.Add(new ParseError("Expected end of line or file", token));
                    return new ParseOperationResult(node);
                }
            }

            return new ParseOperationResult(node, true);
        }

        private ParseOperationResult ParseFunctionArgument(ParseResult result)
        {
            var token = this.ScanNextToken();
            if (!this.parseFunctions.TryGetValue(token.Type, out Func<ParseResult, ParseOperationResult> parseFunction))
            {
                result.Errors.Add(new ParseError("Unable to parse function arg " + token.Type.ToString(), token));
                return new ParseOperationResult(null);
            }

            this.UnscanToken();
            var child = parseFunction(result);
            if (!child.IsValid) return new ParseOperationResult(null);
            return new ParseOperationResult(child.Node, true);
        }

        private ParseOperationResult ParseFunction(ParseResult result)
        {
            return this.ParseFunctionItem(result, true);
        }

        private ParseOperationResult ParseConstant(ParseResult result)
        {
            var token = this.ScanNextToken();
            return new ParseOperationResult(this.MakeAstNode(AstNodeType.Constant, token), true);
        }

        private ParseOperationResult ParseVariable(ParseResult result)
        {
            var token = this.ScanNextToken();
            return new ParseOperationResult(this.MakeAstNode(AstNodeType.Variable, token), true);
        }

        private void ClearToNewLine()
        {
            var token = this.ScanNextToken();
            while ((token.Type != TokenType.EOF) && (token.Type != TokenType.Newline))
            {
                token = this.ScanNextToken();
            }

            this.UnscanToken();
        }

        private Token ScanNextToken()
        {
            var token = this.ScanToken();
            while (true)
            {
                if (token.Type == TokenType.SourceID)
                {
                    this.lastSourceId = token;
                }
                else if (token.Type != TokenType.Whitespace)
                {
                    break;
                }

                token = this.ScanToken();
            }

            return token;
        }

        private Token ScanToken()
        {
            if (this.hasCached)
            {
                this.hasCached = false;
                return this.lastToken;
            }

            this.lastToken = this.scanner.Read();
            return this.lastToken;
        }

        private void UnscanToken()
        {
            this.hasCached = true;
        }

        private AstNode MakeAstNode(AstNodeType type, Token token)
        {
            var node = new AstNode(type, token, this.lastSourceId?.Value);
            this.lastSourceId = null;
            return node;
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
