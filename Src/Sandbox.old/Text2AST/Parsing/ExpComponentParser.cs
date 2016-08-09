using System;
using Gum.Lang.AbstractSyntax;

namespace Gum.Translator.Text2AST.Parsing
{
    class ExpComponentParser : Parser<IExpComponent>
    {
        protected override IExpComponent ParseInner(Lexer lexer)
        {
            return Parse<IExpComponent, AssignExpParser>(lexer);
        }
    }
}