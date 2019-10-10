﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NaoBlocks.Parser
{
    public class Token
    {
        public Token(TokenType type, string value)
        {
            this.Type = type;
            this.Value = value;
        }

        public TokenType Type { get; set; }

        public string Value { get; set; }

        public int LineNumber { get; set; }

        public int LinePosition { get; set; }
    }
}
