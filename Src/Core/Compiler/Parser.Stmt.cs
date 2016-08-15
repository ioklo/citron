using Gum.Data.AbstractSyntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Compiler
{
    partial class Parser
    {
        private VarDeclStmt ParseVarDeclStmt()
        {
            TypeID typeID = ParseTypeID();
            var nameAndExps = new List<NameAndExp>();

            do
            {
                string name;
                ConsumeOrThrow(TokenType.Identifier, out name);

                if (Consume(TokenType.Equal))
                {
                    IExpComponent exp = ParseExp();
                    nameAndExps.Add(new NameAndExp(name, exp));
                }
                else
                {
                    nameAndExps.Add(new NameAndExp(name, null));
                }

            } while (Consume(TokenType.Comma));

            ConsumeOrThrow(TokenType.SemiColon);
            return new VarDeclStmt(typeID, nameAndExps);
        }

        private BlockStmt ParseBlockStmt()
        {
            var stmts = new List<IStmtComponent>();
            ConsumeOrThrow(TokenType.LBrace);

            IStmtComponent stmt;
            while (RollbackIfFailed(out stmt, ParseStmt))
                stmts.Add(stmt);

            ConsumeOrThrow(TokenType.RBrace);
            return new BlockStmt(stmts);
        }

        private ExpStmt ParseExpStmt()
        {
            IExpComponent exp = ParseExp();
            ConsumeOrThrow(TokenType.SemiColon);
            return new ExpStmt(exp);
        }

        private IfStmt ParseIfStmt()
        {
            ConsumeOrThrow(TokenType.If);
            ConsumeOrThrow(TokenType.LParen);

            IExpComponent condExp = ParseExp();

            ConsumeOrThrow(TokenType.RParen);

            IStmtComponent thenStmt = ParseStmt();

            // if () if () {} else {}
            if (Consume(TokenType.Else))
            {
                IStmtComponent elseStmt = ParseStmt();
                return new IfStmt(condExp, thenStmt, elseStmt);
            }
            else
            {
                return new IfStmt(condExp, thenStmt, null);
            }
        }

        private ReturnStmt ParseReturnStmt()
        {
            ConsumeOrThrow(TokenType.Return);

            if (Consume(TokenType.SemiColon))
                return new ReturnStmt(null);

            IExpComponent exp = ParseExp();

            ConsumeOrThrow(TokenType.SemiColon);

            return new ReturnStmt(exp);
        }

        private ForStmt ParseForStmt()
        {
            ConsumeOrThrow(TokenType.For);
            ConsumeOrThrow(TokenType.LParen);

            // 첫번째에 들어갈 수 있는 것들..
            // Nothing
            // Variable Declaration With Initial value
            // Expression

            IForInitComponent forInit = ParseForInit();

            ConsumeOrThrow(TokenType.SemiColon);

            // 두번쨰에 들어갈 수 있는 것, IExp
            IExpComponent condExp = ParseExp();

            ConsumeOrThrow(TokenType.SemiColon);

            // 세번째에 들어갈 수 있는 것, IExp
            IExpComponent loopExp = ParseExp();

            ConsumeOrThrow(TokenType.RParen);

            IStmtComponent body = ParseStmt();

            return new ForStmt(forInit, condExp, loopExp, body);
        }

        private ContinueStmt ParseContinueStmt()
        {
            ConsumeOrThrow(TokenType.Continue);
            ConsumeOrThrow(TokenType.SemiColon);

            return new ContinueStmt();
        }

        private BreakStmt ParseBreakStmt()
        {
            ConsumeOrThrow(TokenType.Break);
            ConsumeOrThrow(TokenType.SemiColon);
            return new BreakStmt();
        }

        private WhileStmt ParseWhileStmt()
        {
            ConsumeOrThrow(TokenType.While);
            ConsumeOrThrow(TokenType.LParen);
            IExpComponent condExp = ParseExp();
            ConsumeOrThrow(TokenType.RParen);

            IStmtComponent body;
            RollbackIfFailed(out body, ParseStmt);

            return new WhileStmt(condExp, body);
        }

        private DoWhileStmt ParseDoWhileStmt()
        {
            ConsumeOrThrow(TokenType.Do);

            IStmtComponent body = ParseStmt();

            ConsumeOrThrow(TokenType.While);
            ConsumeOrThrow(TokenType.LParen);

            IExpComponent condExp = ParseExp();

            ConsumeOrThrow(TokenType.RParen);
            ConsumeOrThrow(TokenType.SemiColon);

            return new DoWhileStmt(body, condExp);
        }

        private IStmtComponent ParseStmt()
        {
            if (Consume(TokenType.SemiColon))
                return new BlankStmt();

            // typeID로 시작
            VarDeclStmt varDeclStmt;
            if (RollbackIfFailed(out varDeclStmt, ParseVarDeclStmt))
                return varDeclStmt;

            BlockStmt blockStmt;
            if (RollbackIfFailed(out blockStmt, ParseBlockStmt))
                return blockStmt;

            // TODO: 여기 conflict 날 수 있음
            ExpStmt expStmt;
            if (RollbackIfFailed(out expStmt, ParseExpStmt))
                return expStmt;

            IfStmt ifStmt;
            if (RollbackIfFailed(out ifStmt, ParseIfStmt))
                return ifStmt;

            ReturnStmt returnStmt;
            if (RollbackIfFailed(out returnStmt, ParseReturnStmt))
                return returnStmt;

            ForStmt forStmt;
            if (RollbackIfFailed(out forStmt, ParseForStmt))
                return forStmt;

            ContinueStmt contStmt;
            if (RollbackIfFailed(out contStmt, ParseContinueStmt))
                return contStmt;

            BreakStmt breakStmt;
            if (RollbackIfFailed(out breakStmt, ParseBreakStmt))
                return breakStmt;

            WhileStmt whileStmt;
            if (RollbackIfFailed(out whileStmt, ParseWhileStmt))
                return whileStmt;

            DoWhileStmt doWhileStmt;
            if (RollbackIfFailed(out doWhileStmt, ParseDoWhileStmt))
                return doWhileStmt;

            throw CreateException();
        }
    }
}
