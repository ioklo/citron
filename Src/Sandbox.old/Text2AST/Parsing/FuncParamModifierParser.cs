using System;
using Gum.Lang.AbstractSyntax;

namespace Gum.Translator.Text2AST.Parsing
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