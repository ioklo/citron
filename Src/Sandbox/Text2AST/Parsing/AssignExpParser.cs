using System;
using Gum.Lang.AbstractSyntax;
using System.Collections.Generic;

namespace Gum.Translator.Text2AST.Parsing
{
    // 15. Assignment and (lambda expression '=>', not supported), right associative
    // = *= /= %= += -= <<= >>= &= ^= |= 
    class AssignExpParser : RightAssocBinExpParser<ConditionalORExpParser>
    {
        TokenType[] tokenTypes = new[] 
        {
            TokenType.Equal, TokenType.StarEqual, TokenType.SlashEqual, TokenType.PercentEqual,
            TokenType.PlusEqual, TokenType.MinusEqual, TokenType.LessLessEqual, TokenType.GreaterGreaterEqual,
            TokenType.AmperEqual, TokenType.CaretEqual, TokenType.BarEqual
        };

        protected override TokenType[] OpTokenTypes { get { return tokenTypes; } }
    }
}