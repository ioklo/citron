using System;
using Gum.Data.AbstractSyntax;

namespace Gum.Compiler
{
    partial class Parser
    {
        class DoWhileStmtParser : Parser<DoWhileStmt>
        {
            protected override DoWhileStmt ParseInner(Lexer lexer)
            {
                if (!lexer.Consume(TokenType.Do)) return null;

                IStmtComponent body = Parse<IStmtComponent, StmtComponentParser>(lexer);
                if (body == null)
                    throw new ParsingFailedException<DoWhileStmtParser, IStmtComponent>();

                if (!lexer.Consume(TokenType.While))
                    throw new ParsingTokenFailedException<DoWhileStmtParser>(TokenType.While);

                if (!lexer.Consume(TokenType.LParen))
                    throw new ParsingTokenFailedException<DoWhileStmtParser>(TokenType.LParen);

                IExpComponent condExp = Parse<IExpComponent, ExpComponentParser>(lexer);
                if (condExp == null)
                    throw new ParsingFailedException<DoWhileStmtParser, IExpComponent>();

                if (!lexer.Consume(TokenType.RParen))
                    throw new ParsingTokenFailedException<DoWhileStmtParser>(TokenType.RParen);

                if (!lexer.Consume(TokenType.SemiColon))
                    throw new ParsingTokenFailedException<DoWhileStmtParser>(TokenType.SemiColon);

                return new DoWhileStmt(body, condExp);
            }

        }
    }
}