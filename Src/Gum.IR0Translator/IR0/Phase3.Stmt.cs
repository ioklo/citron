using Gum.CompileTime;
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
        // var a = 0, b = "string"
        
        // int a = 0, b = 2;
        StmtResult VisitVarDeclStmt(S.VarDeclStmt varDeclStmt) 
        {
            // Visit
            VisitVarDecl(varDeclStmt.VarDecl);

            var result = analyzer.AnalyzeVarDeclStmt();

            return new StmtResult(result);
        }

        // 직접 호출이 아니라 IfStmt로부터 호출된다
        StmtResult VisitIfTestStmt(S.IfStmt ifStmt)
        {
            var condResult = VisitExp(ifStmt.Cond, null);
            var result = analyzer.AnalyzeIfTestStmt(ifStmt);
            return new StmtResult(result);
        }

        StmtResult VisitIfStmt(S.IfStmt ifStmt)
        {
            
        }

        StmtResult VisitForStmt(S.ForStmt forStmt) { throw new NotImplementedException(); }
        StmtResult VisitContinueStmt(S.ContinueStmt continueStmt) { throw new NotImplementedException(); }
        StmtResult VisitBreakStmt(S.BreakStmt breakStmt) { throw new NotImplementedException(); }
        StmtResult VisitReturnStmt(S.ReturnStmt returnStmt) { throw new NotImplementedException(); }
        StmtResult VisitBlockStmt(S.BlockStmt blockStmt) { throw new NotImplementedException(); }
        StmtResult VisitBlankStmt(S.BlankStmt blankStmt) 
        {
            var result = analyzer.AnalyzeBlankStmt(blankStmt);
            return new StmtResult(result);
        }
        StmtResult VisitExpStmt(S.ExpStmt expStmt) { throw new NotImplementedException(); }
        StmtResult VisitTaskStmt(S.TaskStmt taskStmt) { throw new NotImplementedException(); }
        StmtResult VisitAwaitStmt(S.AwaitStmt awaitStmt) { throw new NotImplementedException(); }
        StmtResult VisitAsyncStmt(S.AsyncStmt asyncStmt) { throw new NotImplementedException(); }
        StmtResult VisitForeachStmt(S.ForeachStmt foreachStmt) { throw new NotImplementedException(); }
        StmtResult VisitYieldStmt(S.YieldStmt yieldStmt) { throw new NotImplementedException(); }

        StmtResult VisitStmt(S.Stmt stmt)
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
