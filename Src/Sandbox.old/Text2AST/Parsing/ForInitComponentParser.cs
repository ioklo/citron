using System;
using Gum.Lang.AbstractSyntax;

namespace Gum.Translator.Text2AST.Parsing
{
    class ForInitComponentParser : Parser<IForInitComponent>
    {
        protected override IForInitComponent ParseInner(Lexer lexer)
        {
            IForInitComponent exp = Parse<IExpComponent, ExpComponentParser>(lexer) as IForInitComponent;
            if (exp != null)
                return exp;

            VarDecl varDecl = Parse<VarDecl, VarDeclParser>(lexer);
            if (varDecl != null)
                return varDecl;

            return null;
        }
    }
}