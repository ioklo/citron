namespace Citron.Syntax
{
    public interface IStmtVisitor<out TResult>
    {
        TResult VisitEmbeddable(EmbeddableStmt stmt);

        TResult VisitCommand(CommandStmt stmt);
        TResult VisitVarDecl(VarDeclStmt stmt);
        TResult VisitIf(IfStmt stmt);
        TResult VisitIfTest(IfTestStmt stmt);
        TResult VisitFor(ForStmt stmt);

        TResult VisitContinue(ContinueStmt stmt);
        TResult VisitBreak(BreakStmt stmt);
        TResult VisitReturn(ReturnStmt stmt);
        TResult VisitBlock(BlockStmt stmt);
        TResult VisitBlank(BlankStmt stmt);
        TResult VisitExp(ExpStmt stmt);

        TResult VisitTask(TaskStmt stmt);
        TResult VisitAwait(AwaitStmt stmt);
        TResult VisitAsync(AsyncStmt stmt);
        TResult VisitForeach(ForeachStmt stmt);
        TResult VisitYield(YieldStmt stmt);
        
        TResult VisitDirective(DirectiveStmt stmt);
    }
}