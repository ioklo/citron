using System;
using Gum.Lang.AbstractSyntax;

namespace Gum.Translator.Text2AST.Parsing
{
    // 12. ||, left associativity
    class ConditionalORExpParser : LeftAssocBinExpParser<ConditionalANDExpParser>
    {
        private TokenType[] tokenTypes = new [] { TokenType.BarBar };
        protected override TokenType[] OpTokenTypes { get { return tokenTypes; } }
    }
}