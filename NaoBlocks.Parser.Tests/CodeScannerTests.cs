using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Parser.Tests
{
    public class CodeScannerTests
    {
        [Theory]
        [InlineData("", TokenType.EOF, "")]
        [InlineData(" ", TokenType.Whitespace, " ")]
        [InlineData("\t", TokenType.Whitespace, "\t")]
        [InlineData("\r", TokenType.Whitespace, "\r")]
        [InlineData("  ", TokenType.Whitespace, "  ")]
        [InlineData("\n", TokenType.Newline, "\n")]
        [InlineData(",", TokenType.Comma, ",")]
        [InlineData("(", TokenType.OpenBracket, "(")]
        [InlineData(")", TokenType.CloseBracket, ")")]
        [InlineData("{", TokenType.OpenBrace, "{")]
        [InlineData("}", TokenType.CloseBrace, "}")]
        [InlineData("@value", TokenType.Variable, "value")]
        [InlineData("#abcdef", TokenType.Colour, "abcdef")]
        [InlineData("#12ab34", TokenType.Colour, "12ab34")]
        [InlineData("#123456", TokenType.Colour, "123456")]
        [InlineData("test", TokenType.Identifier, "test")]
        [InlineData("TRUE", TokenType.Boolean, "TRUE")]
        [InlineData("FALSE", TokenType.Boolean, "FALSE")]
        [InlineData("TEST", TokenType.Constant, "TEST")]
        [InlineData("CHEST", TokenType.Constant, "CHEST")]
        [InlineData("1", TokenType.Number, "1")]
        [InlineData("-1", TokenType.Number, "-1")]
        [InlineData("1.0", TokenType.Number, "1.0")]
        [InlineData("1.0.1", TokenType.Illegal, "1.0.1")]
        [InlineData("-1.0", TokenType.Number, "-1.0")]
        [InlineData("-1.0.1", TokenType.Illegal, "-1.0.1")]
        [InlineData("'Test'", TokenType.Text, "Test")]
        [InlineData("'TEST'", TokenType.Text, "TEST")]
        [InlineData("'T\\'est'", TokenType.Text, "T\'est")]
        [InlineData("'Test", TokenType.Illegal, "'Test")]
        [InlineData("[abCD+-\\'12(){}]", TokenType.SourceID, "abCD+-\\'12(){}")]
        [InlineData("[abCD+-\\'12(){}", TokenType.Illegal, "[abCD+-\\'12(){}")]
        public async Task ReadTokenTheoryAsync(string input, TokenType expectedType, string expectedValue)
        {
            var reader = new StringReader(input);
            var scanner = new CodeScanner(reader);
            var token = await scanner.ReadAsync();
            var expected = new Token(expectedType, expectedValue);
            Assert.Equal(expected, token, new TokenComparer());
        }

        [Theory]
        [InlineData("say('hello')", TokenType.Identifier, TokenType.OpenBracket, TokenType.Text, TokenType.CloseBracket, TokenType.EOF)]
        [InlineData("say(@hello)", TokenType.Identifier, TokenType.OpenBracket, TokenType.Variable, TokenType.CloseBracket, TokenType.EOF)]
        [InlineData("say(round(1))", TokenType.Identifier, TokenType.OpenBracket, TokenType.Identifier, TokenType.OpenBracket, TokenType.Number, TokenType.CloseBracket, TokenType.CloseBracket, TokenType.EOF)]
        [InlineData("say('hello')\nrest()", TokenType.Identifier, TokenType.OpenBracket, TokenType.Text, TokenType.CloseBracket, TokenType.Newline, TokenType.Identifier, TokenType.OpenBracket, TokenType.CloseBracket, TokenType.EOF)]
        public async Task ReadTokenTypeSequenceTheoryAsync(string input, params TokenType[] expected)
        {
            var reader = new StringReader(input);
            var scanner = new CodeScanner(reader);
            var token = new Token(TokenType.Illegal, "");
            var actual = new List<TokenType>();
            while (token.Type != TokenType.EOF)
            {
                token = await scanner.ReadAsync();
                actual.Add(token.Type);
            }

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("say('hello')", 0, 3, 4, 11, 12)]
        [InlineData("say(@hello)", 0, 3, 4, 10, 11)]
        [InlineData("say(round(1))", 0, 3, 4, 9, 10, 11, 12, 13)]
        [InlineData("say('hello')\nrest()", 0, 3, 4, 11, 12, 0, 4, 5, 6)]
        public async Task ReadLinePositionTheoryAsync(string input, params int[] expected)
        {
            var reader = new StringReader(input);
            var scanner = new CodeScanner(reader);
            var token = new Token(TokenType.Illegal, "");
            var actual = new List<int>();
            while (token.Type != TokenType.EOF)
            {
                token = await scanner.ReadAsync();
                actual.Add(token.LinePosition);
            }

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("say('hello')", 0, 0, 0, 0, 0)]
        [InlineData("say(@hello)", 0, 0, 0, 0, 0)]
        [InlineData("say(round(1))", 0, 0, 0, 0, 0, 0, 0, 0)]
        [InlineData("say('hello')\nrest()", 0, 0, 0, 0, 0, 1, 1, 1, 1)]
        public async Task ReadLineNumberTheoryAsync(string input, params int[] expected)
        {
            var reader = new StringReader(input);
            var scanner = new CodeScanner(reader);
            var token = new Token(TokenType.Illegal, "");
            var actual = new List<int>();
            while (token.Type != TokenType.EOF)
            {
                token = await scanner.ReadAsync();
                actual.Add(token.LineNumber);
            }

            Assert.Equal(expected, actual);
        }
    }
}
