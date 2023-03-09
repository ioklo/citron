namespace Citron.IR0;

// algebraic data type별로 visitor가 하나씩 생긴다
public interface IIR0StmtVisitor
{
    void VisitCommand(CommandStmt stmt);
    void VisitLocalVarDecl(LocalVarDeclStmt stmt);
    void VisitLocalRefVarDecl(LocalRefVarDeclStmt localRefVarDeclStmt);
    void VisitIf(IfStmt stmt);
    void VisitIfTestClass(IfTestClassStmt stmt);
    void VisitIfTestEnumElem(IfTestEnumElemStmt stmt);
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
    void VisitCallClassConstructor(CallClassConstructorStmt stmt);
    void VisitCallStructConstructor(CallStructConstructorStmt stmt);
    void VisitDirective(DirectiveStmt stmt);
}
