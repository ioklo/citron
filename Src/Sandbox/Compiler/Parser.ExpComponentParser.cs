using System;
using Gum.Data.AbstractSyntax;

namespace Gum.Compiler
{
    partial class Parser
    {
        class ExpComponentParser : Parser<IExpComponent>
        {
            protected override IExpComponent ParseInner(Lexer lexer)
            {
                return Parse<IExpComponent, AssignExpParser>(lexer);
            }
        }
    }
}