using System;
using Gum.Data.AbstractSyntax;

namespace Gum.Compiler
{
    partial class Parser
    {
        class WhileStmtParser : Parser<WhileStmt>
        {
            protected override WhileStmt ParseInner(Lexer lexer)
            {
                if (!lexer.Consume(TokenType.While))
                    return null;

                if (!lexer.Consume(TokenType.LParen))
                    throw new ParsingTokenFailedException<WhileStmtParser>(TokenType.LParen);

                IExpComponent condExp = Parse<IExpComponent, ExpComponentParser>(lexer);
                if (condExp == null)
                    throw new ParsingFailedException<WhileStmtParser, IExpComponent>();

                if (!lexer.Consume(TokenType.RParen))
                    throw new ParsingTokenFailedException<WhileStmtParser>(TokenType.RParen);

                IStmtComponent body = Parse<IStmtComponent, StmtComponentParser>(lexer);
                return new WhileStmt(condExp, body);
            }

        }
    }
}