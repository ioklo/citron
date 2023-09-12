namespace Citron.IR0;

// algebraic data type별로 visitor가 하나씩 생긴다
public interface IIR0StmtVisitor<TResult>
{
    TResult VisitCommand(CommandStmt stmt);
    TResult VisitLocalVarDecl(LocalVarDeclStmt stmt);
    TResult VisitIf(IfStmt stmt);
    TResult VisitIfNullableRefTestStmt(IfNullableRefTestStmt stmt);
    TResult VisitIfNullableValueTestStmt(IfNullableValueTestStmt stmt);
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
    TResult VisitCallClassConstructor(CallClassConstructorStmt stmt);
    TResult VisitCallStructConstructor(CallStructConstructorStmt stmt);
    TResult VisitDirective(DirectiveStmt stmt);
}
