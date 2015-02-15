using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.App.Compiler
{
    public class KeywordInfo
    {
        public string Token { get; private set; }
        public Gum.App.Compiler.Lexer.TokenKind Kind { get; private set; }

        public KeywordInfo(string tk, Gum.App.Compiler.Lexer.TokenKind kind)
        {
            Token = tk;
            Kind = kind;
        }
    }
}
