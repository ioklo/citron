using System;
using Gum.Data.AbstractSyntax;

namespace Gum.Compiler
{
    partial class Parser
    {
        // 12. ||, left associativity
        class ConditionalORExpParser : LeftAssocBinExpParser<ConditionalANDExpParser>
        {
            private TokenType[] tokenTypes = new[] { TokenType.BarBar };
            protected override TokenType[] OpTokenTypes { get { return tokenTypes; } }
        }
    }
}