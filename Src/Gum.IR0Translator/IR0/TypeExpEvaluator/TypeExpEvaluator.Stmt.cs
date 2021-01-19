using Gum.Misc;
using System;
using System.Collections.Generic;
using System.Text;
using S = Gum.Syntax;

namespace Gum.IR0
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

            if (ifStmt.TestType != null)
                VisitTypeExpNoThrow(ifStmt.TestType);

            VisitStmt(ifStmt.Body);

            if (ifStmt.ElseBody != null)
                VisitStmt(ifStmt.ElseBody);
        }

        void VisitForStmtInitializer(S.ForStmtInitializer initializer)
        {
            switch (initializer)
            {
                case S.ExpForStmtInitializer expInit: VisitExp(expInit.Exp); break;
                case S.VarDeclForStmtInitializer varDeclInit: VisitVarDecl(varDeclInit.VarDecl); break;
            }

            throw new UnreachableCodeException();
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
            if (returnStmt.Value != null)
                VisitExp(returnStmt.Value);
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
            VisitTypeExpNoThrow(foreachStmt.Type);
            VisitExp(foreachStmt.Iterator);
            VisitStmt(foreachStmt.Body);
        }

        void VisitYieldStmt(S.YieldStmt yieldStmt)
        {
            VisitExp(yieldStmt.Value);
        }

        void VisitStmt(S.Stmt stmt)
        {
            switch (stmt)
            {
                case S.CommandStmt cmdStmt: VisitCommandStmt(cmdStmt); break;
                case S.VarDeclStmt varDeclStmt: VisitVarDeclStmt(varDeclStmt); break;
                case S.IfStmt ifStmt: VisitIfStmt(ifStmt); break;
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
                default: throw new UnreachableCodeException();
            };
        }
    }
}
