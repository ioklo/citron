using System;
using Gum.Lang.AbstractSyntax;

namespace Gum.Translator.Text2AST.Parsing
{
    class ContinueStmtParser : Parser<ContinueStmt>
    {
        protected override ContinueStmt ParseInner(Lexer lexer)
        {
            if (!lexer.Consume(TokenType.Continue)) return null;

            if (!lexer.Consume(TokenType.SemiColon))
                throw new ParsingTokenFailedException<ContinueStmtParser>(TokenType.SemiColon);

            return new ContinueStmt();
        }


    }
}