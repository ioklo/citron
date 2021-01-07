using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using S = Gum.Syntax;

namespace Gum.IR0
{
    partial class Phase3
    {
        CommandStmt VisitCommandStmt(S.CommandStmt cmdStmt)
        {
            // Visit
            var stringExps = new List<StringExp>();
            foreach (var cmd in cmdStmt.Commands)
            {
                var stringExp = VisitStringExp(cmd);
                stringExps.Add(stringExp);
            }

            return analyzer.AnalyzeCommandStmt(stringExps);
        }

        void VisitVarDeclElement(S.VarDeclElement elem)
        {
            analyzer.AnalyzeVarDeclElement(elem);
        }

        void VisitVarDecl(S.VarDecl varDecl)
        {
            foreach(var elem in varDecl.Elems)
            {
                VisitVarDeclElement(elem);
            }
        }
        
        Stmt VisitVarDeclStmt(S.VarDeclStmt varDeclStmt) 
        {
            // 이렇게까지 나눠져야 할 일인가
            VisitVarDecl(varDeclStmt.VarDecl);
        }

        Stmt VisitIfStmt(S.IfStmt ifStmt) { throw new NotImplementedException(); }
        Stmt VisitForStmt(S.ForStmt forStmt) { throw new NotImplementedException(); }
        Stmt VisitContinueStmt(S.ContinueStmt continueStmt) { throw new NotImplementedException(); }
        Stmt VisitBreakStmt(S.BreakStmt breakStmt) { throw new NotImplementedException(); }
        Stmt VisitReturnStmt(S.ReturnStmt returnStmt) { throw new NotImplementedException(); }
        Stmt VisitBlockStmt(S.BlockStmt blockStmt) { throw new NotImplementedException(); }
        BlankStmt VisitBlankStmt(S.BlankStmt _) { return BlankStmt.Instance; }
        Stmt VisitExpStmt(S.ExpStmt expStmt) { throw new NotImplementedException(); }
        Stmt VisitTaskStmt(S.TaskStmt taskStmt) { throw new NotImplementedException(); }
        Stmt VisitAwaitStmt(S.AwaitStmt awaitStmt) { throw new NotImplementedException(); }
        Stmt VisitAsyncStmt(S.AsyncStmt asyncStmt) { throw new NotImplementedException(); }
        Stmt VisitForeachStmt(S.ForeachStmt foreachStmt) { throw new NotImplementedException(); }
        Stmt VisitYieldStmt(S.YieldStmt yieldStmt) { throw new NotImplementedException(); }

        Stmt VisitStmt(S.Stmt stmt)
        {
            switch (stmt)
            {
                case S.CommandStmt cmdStmt: return VisitCommandStmt(cmdStmt);
                case S.VarDeclStmt varDeclStmt: return VisitVarDeclStmt(varDeclStmt);
                case S.IfStmt ifStmt: return VisitIfStmt(ifStmt);
                case S.ForStmt forStmt: return VisitForStmt(forStmt);
                case S.ContinueStmt continueStmt: return VisitContinueStmt(continueStmt);
                case S.BreakStmt breakStmt: return VisitBreakStmt(breakStmt);
                case S.ReturnStmt returnStmt: return VisitReturnStmt(returnStmt);
                case S.BlockStmt blockStmt: return VisitBlockStmt(blockStmt);
                case S.BlankStmt blankStmt: return VisitBlankStmt(blankStmt);
                case S.ExpStmt expStmt: return VisitExpStmt(expStmt);
                case S.TaskStmt taskStmt: return VisitTaskStmt(taskStmt);
                case S.AwaitStmt awaitStmt: return VisitAwaitStmt(awaitStmt);
                case S.AsyncStmt asyncStmt: return VisitAsyncStmt(asyncStmt);
                case S.ForeachStmt foreachStmt: return VisitForeachStmt(foreachStmt);
                case S.YieldStmt yieldStmt: return VisitYieldStmt(yieldStmt);
                default: throw new InvalidOperationException();
            }
        }
    }
}
