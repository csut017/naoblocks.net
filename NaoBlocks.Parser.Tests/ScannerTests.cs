using System.Collections.Generic;
using System.IO;
using Xunit;

namespace NaoBlocks.Parser.Tests
{
    public class ScannerTests
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
        public void ReadTokenTheory(string input, TokenType expectedType, string expectedValue)
        {
            var reader = new StringReader(input);
            var scanner = new Scanner(reader);
            var token = scanner.Read();
            var expected = new Token(expectedType, expectedValue);
            Assert.Equal(expected, token, new TokenComparer());
        }

        [Theory]
        [InlineData("say('hello')", TokenType.Identifier, TokenType.OpenBracket, TokenType.Text, TokenType.CloseBracket, TokenType.EOF)]
        [InlineData("say(@hello)", TokenType.Identifier, TokenType.OpenBracket, TokenType.Variable, TokenType.CloseBracket, TokenType.EOF)]
        [InlineData("say(round(1))", TokenType.Identifier, TokenType.OpenBracket, TokenType.Identifier, TokenType.OpenBracket, TokenType.Number, TokenType.CloseBracket, TokenType.CloseBracket, TokenType.EOF)]
        public void ReadTokenTypeSequenceTheory(string input, params TokenType[] expected)
        {
            var reader = new StringReader(input);
            var scanner = new Scanner(reader);
            var token = new Token(TokenType.Illegal, "");
            var actual = new List<TokenType>();
            while (token.Type != TokenType.EOF)
            {
                token = scanner.Read();
                actual.Add(token.Type);
            }

            Assert.Equal(expected, actual);
        }
    }
}
