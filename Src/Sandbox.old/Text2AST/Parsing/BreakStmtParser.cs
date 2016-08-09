using System;
using Gum.Lang.AbstractSyntax;

namespace Gum.Translator.Text2AST.Parsing
{
    class BreakStmtParser : Parser<BreakStmt>
    {
        protected override BreakStmt ParseInner(Lexer lexer)
        {
            if (!lexer.Consume(TokenType.Break))
                return null;

            if (!lexer.Consume(TokenType.SemiColon))
                throw new ParsingTokenFailedException<BreakStmtParser>(TokenType.SemiColon);

            return new BreakStmt();
        }

    }
}