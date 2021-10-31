using Gum.IR0Visitor;
using R = Gum.IR0;

namespace Gum.IR0Analyzer
{
    public interface IIR0DataFlowAnalyzer
    {
        // analyzer에 대한 오퍼레이션        
        void PushScope();
        void PopScope();
        IIR0DataFlowAnalyzer Clone();
        void Merge(IIR0DataFlowAnalyzer other);

        void VisitCommandStmt(R.CommandStmt commandStmt);
        void VisitGlobalVarDeclStmt(R.GlobalVarDeclStmt globalVarDeclStmt);
        void VisitLocalVarDeclStmt(R.LocalVarDeclStmt localVarDeclStmt);
        void VisitIfStmt(R.IfStmt ifStmt);
        void VisitIfTestClassStmt(R.IfTestClassStmt ifTestClassStmt);
        void VisitIfTestEnumElemStmt(R.IfTestEnumElemStmt ifTestEnumElemStmt);
        void VisitForStmt(R.ForStmt forStmt);
        void VisitContinueStmt(R.ContinueStmt continueStmt);
        void VisitBreakStmt(R.BreakStmt breakStmt);
        void VisitReturnStmt(R.ReturnStmt returnStmt);
        void VisitBlockStmt(R.BlockStmt blockStmt);
        void VisitBlankStmt(R.BlankStmt blankStmt);
        void VisitExpStmt(R.ExpStmt expStmt);
        void VisitTaskStmt(R.TaskStmt taskStmt);
        void VisitAwaitStmt(R.AwaitStmt awaitStmt);
        void VisitAsyncStmt(R.AsyncStmt asyncStmt);
        void VisitForeachStmt(R.ForeachStmt foreachStmt);
        void VisitYieldStmt(R.YieldStmt yieldStmt);
        void VisitDirectiveStmt(R.DirectiveStmt yieldStmt);
    }
}