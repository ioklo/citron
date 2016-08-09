using System;
using Gum.Data.AbstractSyntax;

namespace Gum.Compiler
{
    partial class Parser
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
}