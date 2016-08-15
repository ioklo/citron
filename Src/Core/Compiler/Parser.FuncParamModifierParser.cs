using System;
using Gum.Data.AbstractSyntax;

namespace Gum.Compiler
{
    partial class Parser
    {
        private FuncParamModifier ParseFuncParamModifier()
        {
            if (Consume(TokenType.Out))
                return FuncParamModifier.Out;

            if (Consume(TokenType.Params))
                return FuncParamModifier.Parameters;

            throw CreateException();
        }
    }
}