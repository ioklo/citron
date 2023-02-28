namespace Citron.Syntax
{
    public interface IStmtVisitor
    {
        void VisitCommand(CommandStmt stmt);
        void VisitVarDecl(VarDeclStmt stmt);
        void VisitIf(IfStmt stmt);
        void VisitIfTest(IfTestStmt stmt);
        void VisitFor(ForStmt stmt);

        void VisitContinue(ContinueStmt stmt);
        void VisitBreak(BreakStmt stmt);
        void VisitReturn(ReturnStmt stmt);
        void VisitBlock(BlockStmt stmt);
        void VisitBlank(BlankStmt stmt);
        void VisitExp(ExpStmt stmt);

        void VisitTask(TaskStmt stmt);
        void VisitAwait(AwaitStmt stmt);
        void VisitAsync(AsyncStmt stmt);
        void VisitForeach(ForeachStmt stmt);
        void VisitYield(YieldStmt stmt);
        
        void VisitDirective(DirectiveStmt stmt);
    }
}