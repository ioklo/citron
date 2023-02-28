using R = Citron.IR0;

namespace Citron.IR0Visitor
{
    // algebraic data type별로 visitor가 하나씩 생긴다
    public interface IIR0StmtVisitor
    {
        void VisitCommandStmt(R.CommandStmt commandStmt);
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
