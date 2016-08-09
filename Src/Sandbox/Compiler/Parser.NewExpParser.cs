using System;
using Gum.Data.AbstractSyntax;

namespace Gum.Compiler
{
    partial class Parser
    {
        class NewExpParser : Parser<IExpComponent>
        {
            protected override IExpComponent ParseInner(Lexer lexer)
            {
                throw new NotImplementedException();
            }
        }
    }
}