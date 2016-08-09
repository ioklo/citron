using System;
using Gum.Lang.AbstractSyntax;

namespace Gum.Translator.Text2AST.Parsing
{
    class ExpStmtParser : Parser<ExpStmt>
    {
        protected override ExpStmt ParseInner(Lexer lexer)
        {
            IExpComponent exp = Parse<IExpComponent, ExpComponentParser>(lexer);
            if (exp == null) return null;

            if (!lexer.Consume(TokenType.SemiColon))
                throw new ParsingTokenFailedException<ExpStmt>(TokenType.SemiColon);

            return new ExpStmt(exp);
        }
    }
}