using S = Citron.Syntax;

namespace Citron.Analysis;

struct StmtVisitor_TopLevel : S.IStmtVisitor
{
    ScopeContext scopeContext;
    CoreStmtVisitor coreVisitor;

    public StmtVisitor_TopLevel(ScopeContext scopeContext)
    {
        this.scopeContext = scopeContext;
        this.coreVisitor = new CoreStmtVisitor(scopeContext);
    }

    public void VisitVarDecl(S.VarDeclStmt stmt)
    {
        // GlobalVariable 처리
    }

    public void VisitAsync(S.AsyncStmt stmt) => coreVisitor.VisitAsync(stmt);
    public void VisitAwait(S.AwaitStmt stmt) => coreVisitor.VisitAwait(stmt);
    public void VisitBlank(S.BlankStmt stmt) => coreVisitor.VisitBlank(stmt);
    public void VisitBlock(S.BlockStmt stmt) => coreVisitor.VisitBlock(stmt);
    public void VisitBreak(S.BreakStmt stmt) => coreVisitor.VisitBreak(stmt);
    public void VisitCommand(S.CommandStmt stmt) => coreVisitor.VisitCommand(stmt);
    public void VisitContinue(S.ContinueStmt stmt) => coreVisitor.VisitContinue(stmt);
    public void VisitDirective(S.DirectiveStmt stmt) => coreVisitor.VisitDirective(stmt);
    public void VisitExp(S.ExpStmt stmt) => coreVisitor.VisitExp(stmt);
    public void VisitFor(S.ForStmt stmt) => coreVisitor.VisitFor(stmt);
    public void VisitForeach(S.ForeachStmt stmt) => coreVisitor.VisitForeach(stmt);
    public void VisitIf(S.IfStmt stmt) => coreVisitor.VisitIf(stmt);
    public void VisitIfTest(S.IfTestStmt stmt) => coreVisitor.VisitIfTest(stmt);
    public void VisitReturn(S.ReturnStmt stmt) => coreVisitor.VisitReturn(stmt);
    public void VisitTask(S.TaskStmt stmt) => coreVisitor.VisitTask(stmt);
    public void VisitYield(S.YieldStmt stmt) => coreVisitor.VisitYield(stmt);
}
