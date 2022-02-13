using Citron.IR0Visitor;
using System;
using R = Citron.IR0;

// TODO: 그냥 모듈 이름이랑 맞추는게 나을 것 같다
namespace Citron.IR0Analyzer
{
    // 지정된 순서대로 순회한다
    public class IR0DataFlowAnalyzerExtension : IIR0StmtVisitor
    {
        IIR0DataFlowAnalyzer analyzer;

        public IR0DataFlowAnalyzerExtension(IIR0DataFlowAnalyzer analyzer)
        {
            this.analyzer = analyzer;
        }

        // 일단 interface로 하고, 나중에 struct ref 버전을 만들어 보자
        public static void AnalyzeScript(R.Script script, IIR0DataFlowAnalyzer analyzer)
        {
            var extension = new IR0DataFlowAnalyzerExtension(analyzer);

            foreach (var stmt in script.TopLevelStmts)
                extension.Visit(stmt);
        }

        void IIR0StmtVisitor.VisitAsyncStmt(R.AsyncStmt asyncStmt)
        {
            analyzer.VisitAsyncStmt(asyncStmt);
        }

        void IIR0StmtVisitor.VisitAwaitStmt(R.AwaitStmt awaitStmt)
        {
            analyzer.VisitAwaitStmt(awaitStmt);

            this.Visit(awaitStmt.Body);
        }

        void IIR0StmtVisitor.VisitBlankStmt(R.BlankStmt blankStmt)
        {
            analyzer.VisitBlankStmt(blankStmt);
        }

        void IIR0StmtVisitor.VisitBlockStmt(R.BlockStmt blockStmt)
        {
            analyzer.VisitBlockStmt(blockStmt);

            analyzer.PushScope();

            foreach (var stmt in blockStmt.Stmts)
                this.Visit(stmt);

            analyzer.PopScope();
        }

        void IIR0StmtVisitor.VisitBreakStmt(R.BreakStmt breakStmt)
        {
            analyzer.VisitBreakStmt(breakStmt);
        }

        void IIR0StmtVisitor.VisitCommandStmt(R.CommandStmt commandStmt)
        {
            // exp들은 어떻게 처리할것인가;
            analyzer.VisitCommandStmt(commandStmt);
        }

        void IIR0StmtVisitor.VisitContinueStmt(R.ContinueStmt continueStmt)
        {
            throw new NotImplementedException();
        }

        void IIR0StmtVisitor.VisitDirectiveStmt(R.DirectiveStmt yieldStmt)
        {
            throw new NotImplementedException();
        }

        void IIR0StmtVisitor.VisitExpStmt(R.ExpStmt expStmt)
        {
            throw new NotImplementedException();
        }

        void IIR0StmtVisitor.VisitForeachStmt(R.ForeachStmt foreachStmt)
        {
            throw new NotImplementedException();
        }

        void IIR0StmtVisitor.VisitForStmt(R.ForStmt forStmt)
        {
            throw new NotImplementedException();
        }

        void IIR0StmtVisitor.VisitGlobalVarDeclStmt(R.GlobalVarDeclStmt globalVarDeclStmt)
        {
            throw new NotImplementedException();
        }

        void IIR0StmtVisitor.VisitIfStmt(R.IfStmt ifStmt)
        {
            throw new NotImplementedException();
        }

        void IIR0StmtVisitor.VisitIfTestClassStmt(R.IfTestClassStmt ifTestClassStmt)
        {
            throw new NotImplementedException();
        }

        void IIR0StmtVisitor.VisitIfTestEnumElemStmt(R.IfTestEnumElemStmt ifTestEnumElemStmt)
        {
            throw new NotImplementedException();
        }

        void IIR0StmtVisitor.VisitLocalVarDeclStmt(R.LocalVarDeclStmt localVarDeclStmt)
        {
            throw new NotImplementedException();
        }

        void IIR0StmtVisitor.VisitReturnStmt(R.ReturnStmt returnStmt)
        {
            throw new NotImplementedException();
        }

        void IIR0StmtVisitor.VisitTaskStmt(R.TaskStmt taskStmt)
        {
            throw new NotImplementedException();
        }

        void IIR0StmtVisitor.VisitYieldStmt(R.YieldStmt yieldStmt)
        {
            throw new NotImplementedException();
        }
    }
}
