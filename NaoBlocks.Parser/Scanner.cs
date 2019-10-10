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
            var inputChar = this.read();

            if (inputChar < 0)
            {
                return new Token(TokenType.EOF, "");
            }

            var charToCheck = (char)inputChar;
            if (this._chars.TryGetValue(charToCheck, out TokenType tokenType)) return new Token(tokenType, charToCheck.ToString());
            if (char.IsWhiteSpace(charToCheck)) return this.generateWhitespaceToken();
            if (char.IsLetter(charToCheck))
            {
                this.unread();
                return this.generateItem(TokenType.Identifier, true);
            }
            if (char.IsDigit(charToCheck) || (charToCheck == '-')) return this.generateNumber(charToCheck);
            if (charToCheck == '@') return this.generateItem(TokenType.Variable, false);
            if (charToCheck == '#') return this.generateItem(TokenType.Colour, false);
            if (charToCheck == '\'') return this.generateText();
            if (charToCheck == '[') return this.generateSourceID();

            return new Token(TokenType.Illegal, charToCheck.ToString());
        }

        private int read()
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

        private void unread()
        {
            this._hasChar = true;
        }

        private Token generateWhitespaceToken()
        {
            var builder = new StringBuilder();
            builder.Append((char)this._lastChar);
            while (true)
            {
                var nextChar = this.read();
                if (nextChar < 0) break;

                var charToCheck = (char)nextChar;
                if (char.IsWhiteSpace(charToCheck) && (charToCheck != '\n'))
                {
                    builder.Append(charToCheck);
                }
                else
                {
                    this.unread();
                    break;
                }
            }

            return new Token(TokenType.Whitespace, builder.ToString());
        }

        private Token generateNumber(char firstChar)
        {
            var builder = new StringBuilder();
            builder.Append(firstChar);
            var hasDecimal = false;
            var isLegal = true;
            while (true)
            {
                var nextChar = this.read();
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
                    this.unread();
                    break;
                }
            }

            return new Token(isLegal ? TokenType.Number : TokenType.Illegal, builder.ToString());
        }

        private Token generateText()
        {
            var builder = new StringBuilder();
            var isLegal = false;
            while (true)
            {
                var nextChar = this.read();
                if (nextChar < 0) break;

                var charToCheck = (char)nextChar;
                if (charToCheck == '\'')
                {
                    isLegal = true;
                    break;
                }
                else if (charToCheck == '\\')
                {
                    var quoteChar = (char)this.read();
                    if (quoteChar == '\'')
                    {
                        builder.Append('\'');
                    }
                    else
                    {
                        builder.Append('\\');
                        this.unread();
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

        private Token generateSourceID()
        {
            var builder = new StringBuilder();
            var isLegal = false;
            while (true)
            {
                var nextChar = this.read();
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

        private Token generateItem(TokenType tokenType, bool checkIfConstant)
        {
            var value = this.readIdentifier();
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

        private string readIdentifier()
        {
            var builder = new StringBuilder();
            while (true)
            {
                var nextChar = this.read();
                if (nextChar < 0) break;

                var charToCheck = (char)nextChar;
                if (char.IsLetterOrDigit(charToCheck) || (charToCheck == '_'))
                {
                    builder.Append(charToCheck);
                }
                else
                {
                    this.unread();
                    break;
                }
            }
            return builder.ToString();
        }
    }
}
