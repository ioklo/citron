using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Text;
using S = Gum.Syntax;

namespace Gum.IR0Translator
{
    partial class TypeExpEvaluator
    {
        void VisitCommandStmt(S.CommandStmt cmdStmt)
        {
            foreach (var cmd in cmdStmt.Commands)
                VisitStringExpElements(cmd.Elements);
        }

        void VisitVarDeclStmt(S.VarDeclStmt varDeclStmt)
        {
            VisitVarDecl(varDeclStmt.VarDecl);
        }

        void VisitIfStmt(S.IfStmt ifStmt)
        {
            VisitExp(ifStmt.Cond);
            VisitStmt(ifStmt.Body);

            if (ifStmt.ElseBody != null)
                VisitStmt(ifStmt.ElseBody);
        }

        void VisitIfTestStmt(S.IfTestStmt ifTestStmt)
        {
            VisitExp(ifTestStmt.Exp);
            VisitTypeExpOuterMost(ifTestStmt.TestType);
            VisitStmt(ifTestStmt.Body);
            if (ifTestStmt.ElseBody != null)
                VisitStmt(ifTestStmt.ElseBody);
        }

        void VisitForStmtInitializer(S.ForStmtInitializer initializer)
        {
            switch (initializer)
            {
                case S.ExpForStmtInitializer expInit: VisitExp(expInit.Exp); break;
                case S.VarDeclForStmtInitializer varDeclInit: VisitVarDecl(varDeclInit.VarDecl); break;
                default: throw new UnreachableCodeException();
            }            
        }

        void VisitForStmt(S.ForStmt forStmt)
        {
            if (forStmt.Initializer != null)
                VisitForStmtInitializer(forStmt.Initializer);

            if (forStmt.CondExp != null)
                VisitExp(forStmt.CondExp);

            if (forStmt.ContinueExp != null)
                VisitExp(forStmt.ContinueExp);

            VisitStmt(forStmt.Body);
        }

        void VisitContinueStmt(S.ContinueStmt continueStmt)
        {
        }

        void VisitBreakStmt(S.BreakStmt breakStmt)
        {
        }

        void VisitReturnStmt(S.ReturnStmt returnStmt)
        {
            if (returnStmt.Info != null)
                VisitExp(returnStmt.Info.Value.Value);
        }

        void VisitBlockStmt(S.BlockStmt blockStmt)
        {
            foreach (var stmt in blockStmt.Stmts)
                VisitStmt(stmt);
        }

        void VisitExpStmt(S.ExpStmt expStmt)
        {
            VisitExp(expStmt.Exp);
        }

        void VisitTaskStmt(S.TaskStmt taskStmt)
        {
            VisitStmt(taskStmt.Body);
        }

        void VisitAwaitStmt(S.AwaitStmt awaitStmt)
        {
            VisitStmt(awaitStmt.Body);
        }

        void VisitAsyncStmt(S.AsyncStmt asyncStmt)
        {
            VisitStmt(asyncStmt.Body);
        }

        void VisitForeachStmt(S.ForeachStmt foreachStmt)
        {
            VisitTypeExpOuterMost(foreachStmt.Type);
            VisitExp(foreachStmt.Iterator);
            VisitStmt(foreachStmt.Body);
        }

        void VisitYieldStmt(S.YieldStmt yieldStmt)
        {
            VisitExp(yieldStmt.Value);
        }

        void VisitDirectiveStmt(S.DirectiveStmt directiveStmt)
        {
            foreach (var arg in directiveStmt.Args)
                VisitExp(arg);
        }

        void VisitStmt(S.Stmt stmt)
        {
            switch (stmt)
            {
                case S.CommandStmt cmdStmt: VisitCommandStmt(cmdStmt); break;
                case S.VarDeclStmt varDeclStmt: VisitVarDeclStmt(varDeclStmt); break;
                case S.IfStmt ifStmt: VisitIfStmt(ifStmt); break;
                case S.IfTestStmt ifTestStmt: VisitIfTestStmt(ifTestStmt); break;
                case S.ForStmt forStmt: VisitForStmt(forStmt); break;
                case S.ContinueStmt continueStmt: VisitContinueStmt(continueStmt); break;
                case S.BreakStmt breakStmt: VisitBreakStmt(breakStmt); break;
                case S.ReturnStmt returnStmt: VisitReturnStmt(returnStmt); break;
                case S.BlockStmt blockStmt: VisitBlockStmt(blockStmt); break;
                case S.BlankStmt _: break;
                case S.ExpStmt expStmt: VisitExpStmt(expStmt); break;
                case S.TaskStmt taskStmt: VisitTaskStmt(taskStmt); break;
                case S.AwaitStmt awaitStmt: VisitAwaitStmt(awaitStmt); break;
                case S.AsyncStmt asyncStmt: VisitAsyncStmt(asyncStmt); break;
                case S.ForeachStmt foreachStmt: VisitForeachStmt(foreachStmt); break;
                case S.YieldStmt yieldStmt: VisitYieldStmt(yieldStmt); break;
                case S.DirectiveStmt directiveStmt: VisitDirectiveStmt(directiveStmt); break;
                default: throw new UnreachableCodeException();
            };
        }
    }
}
