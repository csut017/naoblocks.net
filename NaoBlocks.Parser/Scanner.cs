using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NaoBlocks.Parser
{
    public class Scanner
    {
        private readonly TextReader _reader;
        private readonly IDictionary<char, TokenType> _chars = new Dictionary<char, TokenType>
        {
            { '\n', TokenType.Newline },
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
        private int _lastChar;
        private bool _hasChar;
        private int _lineNumber;
        private int _linePosition;

        public Scanner(TextReader reader)
        {
            this._reader = reader;
        }

        public Token Read()
        {
            var inputChar = this.ReadNextCharacter();

            if (inputChar < 0)
            {
                return new Token(TokenType.EOF, "");
            }

            var charToCheck = (char)inputChar;
            if (this._chars.TryGetValue(charToCheck, out TokenType tokenType)) return new Token(tokenType, charToCheck.ToString());
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

            return new Token(TokenType.Illegal, charToCheck.ToString());
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

            return new Token(TokenType.Whitespace, builder.ToString());
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

            return new Token(isLegal ? TokenType.Number : TokenType.Illegal, builder.ToString());
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

            if (isLegal) return new Token(TokenType.Text, builder.ToString());
            builder.Insert(0, '\'');
            return new Token(TokenType.Illegal, builder.ToString());
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

            if (isLegal) return new Token(TokenType.SourceID, builder.ToString());
            builder.Insert(0, '[');
            return new Token(TokenType.Illegal, builder.ToString());
        }

        private Token GenerateItem(TokenType tokenType, bool checkIfConstant)
        {
            var value = this.ReadIdentifier();
            if (checkIfConstant)
            {
                if (this._constants.TryGetValue(value, out TokenType constantType)) return new Token(constantType, value);
                if (value.ToUpperInvariant() == value)
                {
                    return new Token(TokenType.Constant, value);
                }
            }

            return new Token(tokenType, value);
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
    }
}
