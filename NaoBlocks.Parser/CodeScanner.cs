using NaoBlocks.Common;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NaoBlocks.Parser
{
    public class CodeScanner
    {
        private readonly IDictionary<char, TokenType> _chars = new Dictionary<char, TokenType>
        {
            {',', TokenType.Comma },
            {'(', TokenType.OpenBracket },
            {')', TokenType.CloseBracket },
            {'{', TokenType.OpenBrace },
            {'}', TokenType.CloseBrace }
        };

        private readonly IDictionary<string, TokenType> _constants = new Dictionary<string, TokenType>
        {
            { "TRUE", TokenType.Boolean },
            { "FALSE", TokenType.Boolean }
        };

        private readonly TextReader _reader;
        private bool _hasChar;
        private int _lastChar;
        private int _lastTokenStart;
        private int _lineNumber;
        private int _linePosition;

        public CodeScanner(TextReader reader)
        {
            this._reader = reader;
        }

        public Task<Token> ReadAsync()
        {
            return Task.FromResult(this.Read());
        }

        private Token GenerateItem(TokenType tokenType, bool checkIfConstant)
        {
            var value = this.ReadIdentifier();
            if (checkIfConstant)
            {
                if (this._constants.TryGetValue(value, out TokenType constantType)) return this.MakeToken(constantType, value);
                if (value.ToUpperInvariant() == value)
                {
                    return this.MakeToken(TokenType.Constant, value);
                }
            }

            return this.MakeToken(tokenType, value);
        }

        private Token GenerateNumber(char firstChar)
        {
            var builder = new StringBuilder();
            builder.Append(firstChar);
            var hasDecimal = false;
            var isLegal = true;
            while (true)
            {
                var nextChar = this.ReadNextCharacter();
                if (nextChar < 0) break;

                var charToCheck = (char)nextChar;
                if (char.IsDigit(charToCheck))
                {
                    builder.Append(charToCheck);
                }
                else if (charToCheck == '.')
                {
                    builder.Append(charToCheck);
                    if (hasDecimal)
                    {
                        isLegal = false;
                    }
                    else
                    {
                        hasDecimal = true;
                    }
                }
                else
                {
                    this.Unread();
                    break;
                }
            }

            var finalNumber = builder.ToString();
            isLegal &= finalNumber != "-";
            return this.MakeToken(isLegal ? TokenType.Number : TokenType.Illegal, finalNumber);
        }

        private Token GenerateSourceID()
        {
            var builder = new StringBuilder();
            var isLegal = false;
            while (true)
            {
                var nextChar = this.ReadNextCharacter();
                if (nextChar < 0) break;

                var charToCheck = (char)nextChar;
                if (charToCheck == ']')
                {
                    isLegal = true;
                    break;
                }
                else
                {
                    builder.Append(charToCheck);
                }
            }

            if (isLegal) return this.MakeToken(TokenType.SourceID, builder.ToString());
            builder.Insert(0, '[');
            return this.MakeToken(TokenType.Illegal, builder.ToString());
        }

        private Token GenerateText()
        {
            var builder = new StringBuilder();
            var isLegal = false;
            while (true)
            {
                var nextChar = this.ReadNextCharacter();
                if (nextChar < 0) break;

                var charToCheck = (char)nextChar;
                if (charToCheck == '\'')
                {
                    isLegal = true;
                    break;
                }
                else if (charToCheck == '\\')
                {
                    var quoteChar = (char)this.ReadNextCharacter();
                    if (quoteChar == '\'')
                    {
                        builder.Append('\'');
                    }
                    else
                    {
                        builder.Append('\\');
                        this.Unread();
                    }
                }
                else
                {
                    builder.Append(charToCheck);
                }
            }

            if (isLegal) return this.MakeToken(TokenType.Text, builder.ToString());
            builder.Insert(0, '\'');
            return this.MakeToken(TokenType.Illegal, builder.ToString());
        }

        private Token GenerateWhitespaceToken()
        {
            var builder = new StringBuilder();
            builder.Append((char)this._lastChar);
            while (true)
            {
                var nextChar = this.ReadNextCharacter();
                if (nextChar < 0) break;

                var charToCheck = (char)nextChar;
                if (char.IsWhiteSpace(charToCheck) && (charToCheck != '\n'))
                {
                    builder.Append(charToCheck);
                }
                else
                {
                    this.Unread();
                    break;
                }
            }

            return this.MakeToken(TokenType.Whitespace, builder.ToString());
        }

        private Token MakeToken(TokenType type, string value)
        {
            return new Token(type, value, this._lineNumber, this._lastTokenStart);
        }

        private Token Read()
        {
            this._lastTokenStart = this._linePosition - (this._hasChar ? 1 : 0);
            var inputChar = this.ReadNextCharacter();

            if (inputChar < 0)
            {
                return this.MakeToken(TokenType.EOF, "");
            }

            var charToCheck = (char)inputChar;
            if (this._chars.TryGetValue(charToCheck, out TokenType tokenType)) return this.MakeToken(tokenType, charToCheck.ToString());
            if (charToCheck == '\n')
            {
                var token = this.MakeToken(TokenType.Newline, "\n");
                this._linePosition = 0;
                this._lineNumber++;
                return token;
            };
            if (char.IsWhiteSpace(charToCheck)) return this.GenerateWhitespaceToken();
            if (char.IsLetter(charToCheck))
            {
                this.Unread();
                return this.GenerateItem(TokenType.Identifier, true);
            }
            if (char.IsDigit(charToCheck) || (charToCheck == '-')) return this.GenerateNumber(charToCheck);
            if (charToCheck == '@') return this.GenerateItem(TokenType.Variable, false);
            if (charToCheck == '#') return this.GenerateItem(TokenType.Colour, false);
            if (charToCheck == '\'') return this.GenerateText();
            if (charToCheck == '[') return this.GenerateSourceID();

            return this.MakeToken(TokenType.Illegal, charToCheck.ToString());
        }

        private string ReadIdentifier()
        {
            var builder = new StringBuilder();
            while (true)
            {
                var nextChar = this.ReadNextCharacter();
                if (nextChar < 0) break;

                var charToCheck = (char)nextChar;
                if (char.IsLetterOrDigit(charToCheck) || (charToCheck == '_'))
                {
                    builder.Append(charToCheck);
                }
                else
                {
                    this.Unread();
                    break;
                }
            }
            return builder.ToString();
        }

        private int ReadNextCharacter()
        {
            if (this._hasChar)
            {
                this._hasChar = false;
                return this._lastChar;
            }

            this._lastChar = this._reader.Read();
            this._linePosition++;
            return this._lastChar;
        }

        private void Unread()
        {
            this._hasChar = true;
        }
    }
}