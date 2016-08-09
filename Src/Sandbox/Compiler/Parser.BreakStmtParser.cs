using System;
using Gum.Data.AbstractSyntax;

namespace Gum.Compiler
{
    partial class Parser
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
}