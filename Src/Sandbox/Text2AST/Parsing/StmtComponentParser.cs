using System;
using Gum.Lang.AbstractSyntax;

namespace Gum.Translator.Text2AST.Parsing
{
    class StmtComponentParser : Parser<IStmtComponent>
    {
        //(varDecl)
        //(blankStmt)
        //(blockStmt)
        //(breakStmt)
        //(continueStmt)
        //(doWhileStmt)
        //(expStmt)
        //(forStmt)
        //(ifStmt)
        //(returnStmt)
        //(whileStmt)

        protected override IStmtComponent ParseInner(Lexer lexer)
        {
            if (lexer.Consume(TokenType.SemiColon))
                return new BlankStmt();

            // typeID로 시작
            VarDeclStmt varDeclStmt = Parse<VarDeclStmt, VarDeclStmtParser>(lexer);
            if (varDeclStmt != null)
                return varDeclStmt;

            BlockStmt blockStmt = Parse<BlockStmt, BlockStmtParser>(lexer);
            if (blockStmt != null)
                return blockStmt;

            // TODO: 여기 conflict 날 수 있음
            ExpStmt expStmt = Parse<ExpStmt, ExpStmtParser>(lexer);
            if (expStmt != null)
                return expStmt;

            IfStmt ifStmt = Parse<IfStmt, IfStmtParser>(lexer);
            if (ifStmt != null)
                return ifStmt;

            ReturnStmt returnStmt = Parse<ReturnStmt, ReturnStmtParser>(lexer);
            if (returnStmt != null)
                return returnStmt;

            ForStmt forStmt = Parse<ForStmt, ForStmtParser>(lexer);
            if (forStmt != null)
                return forStmt;

            ContinueStmt contStmt = Parse<ContinueStmt, ContinueStmtParser>(lexer);
            if (contStmt != null)
                return contStmt;

            BreakStmt breakStmt = Parse<BreakStmt, BreakStmtParser>(lexer);
            if (breakStmt != null)
                return breakStmt;

            WhileStmt whileStmt = Parse<WhileStmt, WhileStmtParser>(lexer);
            if (whileStmt != null)
                return whileStmt;

            DoWhileStmt doWhileStmt = Parse<DoWhileStmt, DoWhileStmtParser>(lexer);
            if (doWhileStmt != null)
                return doWhileStmt;

            return null;        
        }

    }
}