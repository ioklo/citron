using System;
using Gum.Data.AbstractSyntax;
using System.Collections.Generic;

namespace Gum.Compiler
{
    partial class Parser
    {
        class BlockStmtParser : Parser<BlockStmt>
        {
            protected override BlockStmt ParseInner(Lexer lexer)
            {
                var stmts = new List<IStmtComponent>();
                if (!lexer.Consume(TokenType.LBrace))
                    return null;

                IStmtComponent stmt = Parse<IStmtComponent, StmtComponentParser>(lexer);
                while (stmt != null)
                {
                    stmts.Add(stmt);
                    stmt = Parse<IStmtComponent, StmtComponentParser>(lexer);
                }

                if (!lexer.Consume(TokenType.RBrace))
                    throw new ParsingTokenFailedException<BlockStmtParser>(TokenType.RBrace);

                return new BlockStmt(stmts);
            }

        }
    }
}