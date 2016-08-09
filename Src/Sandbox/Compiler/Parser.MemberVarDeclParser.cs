using System;
using Gum.Data.AbstractSyntax;

namespace Gum.Compiler
{
    partial class Parser
    {
        class MemberVarDeclParser : Parser<MemberVarDecl>
        {
            protected override MemberVarDecl ParseInner(Lexer lexer)
            {
                throw new NotImplementedException();
            }
        }
    }
}