using System;
using Gum.Data.AbstractSyntax;

namespace Gum.Compiler
{
    partial class Parser
    {
        class IfStmtParser : Parser<IfStmt>
        {
            protected override IfStmt ParseInner(Lexer lexer)
            {
                if (!lexer.Consume(TokenType.If))
                    return null;

                if (!lexer.Consume(TokenType.LParen))
                    throw new ParsingTokenFailedException<IfStmtParser>(TokenType.LParen);

                IExpComponent condExp = Parse<IExpComponent, ExpComponentParser>(lexer);

                if (condExp == null)
                    throw new ParsingFailedException<IfStmtParser, IExpComponent>();

                if (!lexer.Consume(TokenType.RParen))
                    throw new ParsingTokenFailedException<IfStmtParser>(TokenType.RParen);

                IStmtComponent thenStmt = Parse<IStmtComponent, StmtComponentParser>(lexer);
                if (thenStmt != null)
                    throw new ParsingFailedException<IfStmtParser, IStmtComponent>();

                // if () if () {} else {}
                IStmtComponent elseStmt = null;
                if (lexer.Consume(TokenType.Else))
                {
                    elseStmt = Parse<IStmtComponent, StmtComponentParser>(lexer);
                    if (elseStmt != null)
                        throw new ParsingFailedException<IfStmtParser, IStmtComponent>();
                }

                return new IfStmt(condExp, thenStmt, elseStmt);
            }

        }
    }
}