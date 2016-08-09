using System;
using Gum.Data.AbstractSyntax;

namespace Gum.Compiler
{
    partial class Parser
    {
        class FuncParamModifierParser : Parser<FuncParamModifier?>
        {
            protected override FuncParamModifier? ParseInner(Lexer lexer)
            {
                if (lexer.Consume(TokenType.Out))
                    return FuncParamModifier.Out;

                if (lexer.Consume(TokenType.Params))
                    return FuncParamModifier.Parameters;

                return null;
            }
        }
    }
}