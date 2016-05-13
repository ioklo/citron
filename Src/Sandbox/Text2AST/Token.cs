using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Translator.Text2AST
{
    public class Token
    {
        public TokenType Type { get; private set; }
        public string Value { get; private set; }

        public Token() { }
        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }
        
    }
}
