using System;
using Gum.Data.AbstractSyntax;

namespace Gum.Compiler
{
    partial class Parser
    {
        internal class VarDeclParser : Parser<VarDecl>
        {
            protected override VarDecl ParseInner(Lexer lexer)
            {
                throw new NotImplementedException();
            }
        }
    }
}