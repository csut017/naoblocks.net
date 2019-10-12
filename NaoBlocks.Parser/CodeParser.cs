using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NaoBlocks.Parser
{
    public class CodeParser
    {
        private readonly CodeScanner scanner;
        private readonly IDictionary<string, CompoundFunction> compoundFunctions = new Dictionary<string, CompoundFunction>
        {
            { "if", new CompoundFunction { Name = "if", Clauses = new HashSet<string>(new[] { "elseif", "else" }) } }
        };
        private readonly IDictionary<TokenType, Func<ParseResult, Task<ParseOperationResult>>> parseFunctions;
        private Token lastSourceId;
        private Token lastToken;
        private bool hasCached;

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

        private async Task<ParseOperationResult> ParseItemAsync(ParseResult result)
        {
            var token = await this.ScanNextTokenAsync();
            if (token.Type != TokenType.Identifier)
            {
                result.Errors.Add(new ParseError("Unexpected token", token));
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
                if (parseResult.IsValid) return new ParseOperationResult(compound);

                token = await this.ScanNextTokenAsync();
                this.UnscanToken();
            }

            return new ParseOperationResult(compound, true);
        }

        private async Task<ParseOperationResult> ParseFunctionItemAsync(ParseResult result, bool isArg)
        {
            var token = await this.ScanNextTokenAsync();

            if (token.Type != TokenType.Identifier)
            {
                await this.ClearToNewLine();
                result.Errors.Add(new ParseError("Unexpected token", token));
                return new ParseOperationResult(this.MakeAstNode(AstNodeType.Invalid, token));
            }

            var node = this.MakeAstNode(AstNodeType.Function, token);
            token = await this.ScanNextTokenAsync();
            if (!((token.Type == TokenType.OpenBracket) || (token.Type == TokenType.OpenBrace)))
            {
                await this.ClearToNewLine();
                result.Errors.Add(new ParseError("Unexpected token", token));
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
                    await this.ClearToNewLine();
                    result.Errors.Add(new ParseError("Expected end of line", token));
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
                    result.Errors.Add(new ParseError("Expected end of line or file", token));
                    return new ParseOperationResult(node);
                }
            }

            return new ParseOperationResult(node, true);
        }

        private async Task<ParseOperationResult> ParseFunctionArgumentAsync(ParseResult result)
        {
            var token = await this.ScanNextTokenAsync();
            if (!this.parseFunctions.TryGetValue(token.Type, out Func<ParseResult, Task<ParseOperationResult>> parseFunction))
            {
                result.Errors.Add(new ParseError("Unable to parse function arg " + token.Type.ToString(), token));
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

        private async Task<ParseOperationResult> ParseBracketAsync(ParseResult result)
        {
            var token = await this.ScanNextTokenAsync();
            if (token.Type != TokenType.OpenBracket)
            {
                result.Errors.Add(new ParseError("Expected opening bracket", token));
                return new ParseOperationResult(null);
            }

            var argResult = await this.ParseFunctionArgumentAsync(result);
            if (!argResult.IsValid) return argResult;

            token = await this.ScanNextTokenAsync();
            if (token.Type != TokenType.CloseBracket)
            {
                result.Errors.Add(new ParseError("Expected closing bracket", token));
                return new ParseOperationResult(null);
            }

            return argResult;
        }

        private async Task<ParseOperationResult> ParseConstantAsync(ParseResult result)
        {
            var token = await this.ScanNextTokenAsync();
            return new ParseOperationResult(this.MakeAstNode(AstNodeType.Constant, token), true);
        }

        private async Task<ParseOperationResult> ParseVariableAsync(ParseResult result)
        {
            var token = await this.ScanNextTokenAsync();
            return new ParseOperationResult(this.MakeAstNode(AstNodeType.Variable, token), true);
        }

        private async Task ClearToNewLine()
        {
            var token = await this.ScanNextTokenAsync();
            while ((token.Type != TokenType.EOF) && (token.Type != TokenType.Newline))
            {
                token = await this.ScanNextTokenAsync();
            }

            this.UnscanToken();
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
