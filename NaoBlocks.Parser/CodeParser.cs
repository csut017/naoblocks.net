using NaoBlocks.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NaoBlocks.Parser
{
    public class CodeParser
    {
        private readonly IDictionary<string, CompoundFunction> compoundFunctions = new Dictionary<string, CompoundFunction>
        {
            { "if", new CompoundFunction { Name = "if", Clauses = new HashSet<string>(new[] { "elseif", "else" }) } }
        };

        private readonly IDictionary<TokenType, Func<ParseResult, Task<ParseOperationResult>>> parseFunctions;
        private readonly CodeScanner scanner;
        private bool hasCached;
        private Token lastSourceId;
        private Token lastToken;

        public CodeParser(CodeScanner scanner)
        {
            this.scanner = scanner;
            parseFunctions = new Dictionary<TokenType, Func<ParseResult, Task<ParseOperationResult>>>
                {
                    { TokenType.Boolean, this.ParseConstantAsync },
                    { TokenType.Colour, this.ParseConstantAsync },
                    { TokenType.Constant, this.ParseConstantAsync },
                    { TokenType.Identifier, this.ParseFunctionAsync },
                    { TokenType.OpenBracket, this.ParseBracketAsync },
                    { TokenType.Number, this.ParseConstantAsync },
                    { TokenType.Text, this.ParseConstantAsync },
                    { TokenType.Variable, this.ParseVariableAsync },
                };
        }

        public static CodeParser New(string codeToParse)
        {
            var input = new StringReader(codeToParse);
            var scanner = new CodeScanner(input);
            return new CodeParser(scanner);
        }

        public async Task<ParseResult> ParseAsync()
        {
            var result = new ParseResult();
            var tok = await this.ScanNextTokenAsync();
            if (tok.Type == TokenType.EOF)
            {
                result.Errors.Add(new ParseError("Nothing to parse", tok));
                return result;
            }

            for (; tok.Type != TokenType.EOF; tok = await this.ScanNextTokenAsync())
            {
                if (tok.Type != TokenType.Newline)
                {
                    this.UnscanToken();
                    var res = await this.ParseItemAsync(result);
                    if (res.Node != null) result.Nodes.Add(res.Node);
                }
            }

            return result;
        }

        private async Task ClearToNewLineAsync()
        {
            var token = await this.ScanNextTokenAsync();
            while ((token.Type != TokenType.EOF) && (token.Type != TokenType.Newline))
            {
                token = await this.ScanNextTokenAsync();
            }

            this.UnscanToken();
        }

        private AstNode MakeAstNode(AstNodeType type, Token token)
        {
            var node = new AstNode(type, token, this.lastSourceId?.Value);
            this.lastSourceId = null;
            return node;
        }

        private async Task<ParseOperationResult> ParseBracketAsync(ParseResult result)
        {
            await this.ScanNextTokenAsync();
            var argResult = await this.ParseFunctionArgumentAsync(result);
            if (!argResult.IsValid) return argResult;

            var token = await this.ScanNextTokenAsync();
            if (token.Type != TokenType.CloseBracket)
            {
                result.Errors.Add(new ParseError("Expected closing bracket, got " + token.ToString(), token));
                return new ParseOperationResult(null);
            }

            return argResult;
        }

        private async Task<ParseOperationResult> ParseConstantAsync(ParseResult result)
        {
            var token = await this.ScanNextTokenAsync();
            return new ParseOperationResult(this.MakeAstNode(AstNodeType.Constant, token), true);
        }

        private async Task<ParseOperationResult> ParseFunctionArgumentAsync(ParseResult result)
        {
            var token = await this.ScanNextTokenAsync();
            if (!this.parseFunctions.TryGetValue(token.Type, out Func<ParseResult, Task<ParseOperationResult>> parseFunction))
            {
                result.Errors.Add(new ParseError("Unable to parse function arg " + token.ToString(), token));
                await this.ClearToNewLineAsync();
                return new ParseOperationResult(null);
            }

            this.UnscanToken();
            var child = await parseFunction(result);
            if (!child.IsValid) return new ParseOperationResult(null);
            return new ParseOperationResult(child.Node, true);
        }

        private async Task<ParseOperationResult> ParseFunctionAsync(ParseResult result)
        {
            return await this.ParseFunctionItemAsync(result, true);
        }

        private async Task<ParseOperationResult> ParseFunctionItemAsync(ParseResult result, bool isArg)
        {
            var token = await this.ScanNextTokenAsync();
            var node = this.MakeAstNode(AstNodeType.Function, token);
            token = await this.ScanNextTokenAsync();
            if (!((token.Type == TokenType.OpenBracket) || (token.Type == TokenType.OpenBrace)))
            {
                await this.ClearToNewLineAsync();
                result.Errors.Add(new ParseError("Unexpected token: " + token.ToString(), token));
                return new ParseOperationResult(node);
            }

            if (token.Type == TokenType.OpenBracket)
            {
                token = await this.ScanNextTokenAsync();
                while (token.Type != TokenType.CloseBracket)
                {
                    this.UnscanToken();
                    var argument = await this.ParseFunctionArgumentAsync(result);
                    if (!argument.IsValid) return new ParseOperationResult(node);
                    if (argument.Node != null) node.Arguments.Add(argument.Node);

                    token = await this.ScanNextTokenAsync();
                    if (token.Type == TokenType.Comma) token = await this.ScanNextTokenAsync();
                }

                token = await this.ScanNextTokenAsync();
            }

            if (token.Type == TokenType.OpenBrace)
            {
                token = await this.ScanNextTokenAsync();
                if (token.Type != TokenType.Newline)
                {
                    await this.ClearToNewLineAsync();
                    result.Errors.Add(new ParseError("Expected end of line, got " + token.ToString(), token));
                    return new ParseOperationResult(node);
                }

                token = await this.ScanNextTokenAsync();
                while (token.Type != TokenType.CloseBrace)
                {
                    if (token.Type != TokenType.Newline)
                    {
                        this.UnscanToken();
                        var child = await this.ParseItemAsync(result);
                        if (!child.IsValid) return new ParseOperationResult(node);
                        node.Children.Add(child.Node);
                    }

                    token = await this.ScanNextTokenAsync();
                }
            }
            else
            {
                this.UnscanToken();
            }

            if (!isArg)
            {
                token = await this.ScanNextTokenAsync();
                if (!((token.Type == TokenType.Newline) || (token.Type == TokenType.EOF)))
                {
                    result.Errors.Add(new ParseError("Expected end of line or file, got " + token.ToString(), token));
                    await this.ClearToNewLineAsync();
                    return new ParseOperationResult(node);
                }
            }

            return new ParseOperationResult(node, true);
        }

        private async Task<ParseOperationResult> ParseItemAsync(ParseResult result)
        {
            var token = await this.ScanNextTokenAsync();
            if (token.Type != TokenType.Identifier)
            {
                result.Errors.Add(new ParseError("Unexpected token: " + token.ToString(), token));
                return new ParseOperationResult(this.MakeAstNode(AstNodeType.Invalid, token));
            }

            this.UnscanToken();
            var parseResult = await this.ParseFunctionItemAsync(result, false);
            if (!parseResult.IsValid) return new ParseOperationResult(parseResult.Node);
            if (!this.compoundFunctions.TryGetValue(parseResult.Node.Token.Value, out CompoundFunction function))
            {
                return new ParseOperationResult(parseResult.Node, true);
            }

            var compound = this.MakeAstNode(AstNodeType.Compound, new Token(TokenType.Generated, function.Name));
            compound.Children.Add(parseResult.Node);
            token = await this.ScanNextTokenAsync();
            this.UnscanToken();
            while ((token.Type == TokenType.Identifier) && function.Clauses.Contains(token.Value))
            {
                parseResult = await this.ParseFunctionItemAsync(result, false);
                compound.Children.Add(parseResult.Node);
                if (!parseResult.IsValid) return new ParseOperationResult(compound);

                token = await this.ScanNextTokenAsync();
                this.UnscanToken();
            }

            return new ParseOperationResult(compound, true);
        }

        private async Task<ParseOperationResult> ParseVariableAsync(ParseResult result)
        {
            var token = await this.ScanNextTokenAsync();
            return new ParseOperationResult(this.MakeAstNode(AstNodeType.Variable, token), true);
        }

        private async Task<Token> ScanNextTokenAsync()
        {
            var token = await this.ScanTokenAsync();
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

                token = await this.ScanTokenAsync();
            }

            return token;
        }

        private async Task<Token> ScanTokenAsync()
        {
            if (this.hasCached)
            {
                this.hasCached = false;
                return this.lastToken;
            }

            this.lastToken = await this.scanner.ReadAsync();
            return this.lastToken;
        }

        private void UnscanToken()
        {
            this.hasCached = true;
        }

        private struct CompoundFunction
        {
            public HashSet<string> Clauses { get; set; }
            public string Name { get; set; }
        }

        private struct ParseOperationResult
        {
            public ParseOperationResult(AstNode node, bool isValid = false)
            {
                this.Node = node;
                this.IsValid = isValid;
            }

            public bool IsValid { get; private set; }
            public AstNode Node { get; private set; }
        }
    }
}