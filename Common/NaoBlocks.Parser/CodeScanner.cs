using NaoBlocks.Common;
using System.Text;

namespace NaoBlocks.Parser
{
    /// <summary>
    /// A scanner that will convert a <see cref="TextReader"/> into a series of <see cref="Token"/> instances.
    /// </summary>
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

        /// <summary>
        /// Initialises a new <see cref="CodeScanner"/> instance.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> containing the code.</param>
        public CodeScanner(TextReader reader)
        {
            this._reader = reader;
        }

        private async Task<Token> GenerateItemAsync(TokenType tokenType, bool checkIfConstant)
        {
            var value = await this.ReadIdentifierAsync();
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

        private async Task<Token> GenerateNumberAsync(char firstChar)
        {
            var builder = new StringBuilder();
            builder.Append(firstChar);
            var hasDecimal = false;
            var isLegal = true;
            while (true)
            {
                var nextChar = await this.ReadNextCharacterAsync();
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

        private async Task<Token> GenerateSourceIDAsync()
        {
            var builder = new StringBuilder();
            var isLegal = false;
            while (true)
            {
                var nextChar = await this.ReadNextCharacterAsync();
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

        private async Task<Token> GenerateTextAsync()
        {
            var builder = new StringBuilder();
            var isLegal = false;
            while (true)
            {
                var nextChar = await this.ReadNextCharacterAsync();
                if (nextChar < 0) break;

                var charToCheck = (char)nextChar;
                if (charToCheck == '\'')
                {
                    isLegal = true;
                    break;
                }
                else if (charToCheck == '\\')
                {
                    var quoteChar = (char)await this.ReadNextCharacterAsync();
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

        private async Task<Token> GenerateWhitespaceTokenAsync()
        {
            var builder = new StringBuilder();
            builder.Append((char)this._lastChar);
            while (true)
            {
                var nextChar = await this.ReadNextCharacterAsync();
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

        /// <summary>
        /// Reads the next <see cref="Token"/> from the input source.
        /// </summary>
        /// <returns>The next available <see cref="Token"/>.</returns>
        public async Task<Token> ReadAsync()
        {
            this._lastTokenStart = this._linePosition - (this._hasChar ? 1 : 0);
            var inputChar = await this.ReadNextCharacterAsync();

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
            if (char.IsWhiteSpace(charToCheck)) return await this.GenerateWhitespaceTokenAsync();
            if (char.IsLetter(charToCheck))
            {
                this.Unread();
                return await this.GenerateItemAsync(TokenType.Identifier, true);
            }
            if (char.IsDigit(charToCheck) || (charToCheck == '-')) return await this.GenerateNumberAsync(charToCheck);
            if (charToCheck == '@') return await this.GenerateItemAsync(TokenType.Variable, false);
            if (charToCheck == '#') return await this.GenerateItemAsync(TokenType.Colour, false);
            if (charToCheck == '\'') return await this.GenerateTextAsync();
            if (charToCheck == '[') return await this.GenerateSourceIDAsync();

            return this.MakeToken(TokenType.Illegal, charToCheck.ToString());
        }

        private async Task<string> ReadIdentifierAsync()
        {
            var builder = new StringBuilder();
            while (true)
            {
                var nextChar = await this.ReadNextCharacterAsync();
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

        private async Task<int> ReadNextCharacterAsync()
        {
            if (this._hasChar)
            {
                this._hasChar = false;
                return this._lastChar;
            }

            var buffer = new char[1];
            if (await this._reader.ReadAsync(buffer) > 0)
            {
                this._lastChar = buffer[0];
            }
            else
            {
                this._lastChar = -1;
            }
            this._linePosition++;
            return this._lastChar;
        }

        private void Unread()
        {
            this._hasChar = true;
        }
    }
}