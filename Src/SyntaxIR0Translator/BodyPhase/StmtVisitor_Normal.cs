using System;
using Citron.Symbol;
using Citron.Collections;

using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;

namespace Citron.Analysis;

struct StmtVisitor_Normal : S.IStmtVisitor
{
    ScopeContext context;
    CoreStmtVisitor coreVisitor;

    public StmtVisitor_Normal(ScopeContext context)
    {
        this.context = context;
        this.coreVisitor = new CoreStmtVisitor(context);
    }

    public void VisitVarDecl(S.VarDeclStmt stmt)
    {
        // int a;
        // var x = 
        var varDecl = stmt.VarDecl;
        var declType = context.MakeType(varDecl.Type);

        var visitor = new VarDeclElemVisitor(varDecl.IsRef, declType, context);

        foreach (var elemSyntax in varDecl.Elems)
            visitor.VisitElem(elemSyntax);
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

    public void VisitBody(ImmutableArray<S.Stmt> body)
    {
        foreach (var stmt in body)
        {
            stmt.Accept(ref this);
        }
    }

    public void VisitEmbeddable(S.EmbeddableStmt embedStmt)
    {
        // if (...) 'stmt'
        // if (...) '{ stmt... }' 를 받는다
        switch(embedStmt)
        {
            case S.EmbeddableStmt.Single single:
                // TODO: VarDecl은 등장하면 에러를 내도록 한다
                // 지금은 그냥 패스
                single.Stmt.Accept(ref this);
                return;

            case S.EmbeddableStmt.Multiple multiple:
                foreach (var stmt in multiple.Stmts)
                    stmt.Accept(ref this);
                return;

            default:
                throw new NotImplementedException();
        }
    }

    public void VisitForStmtInitializer(S.ForStmtInitializer forInit)
    {
        switch (forInit)
        {
            case S.VarDeclForStmtInitializer varDeclInit:
                {
                    var declType = context.MakeType(varDeclInit.VarDecl.Type);
                    var visitor = new VarDeclElemVisitor(varDeclInit.VarDecl.IsRef, declType, context);

                    foreach (var elem in varDeclInit.VarDecl.Elems)
                        visitor.VisitElem(elem);
                    break;
                }

            case S.ExpForStmtInitializer expInit:
                {
                    var exp = TranslateAsTopLevelExp(expInit.Exp, hintType: null, A1102_ForStmt_ExpInitializerShouldBeAssignOrCall);
                    context.AddStmt(new R.ExpStmt(exp));
                    break;
                }

            default:
                throw new NotImplementedException();
        }
    }

    public R.Exp TranslateAsTopLevelExp(S.Exp expSyntax, IType? hintType, SyntaxAnalysisErrorCode code) => coreVisitor.TranslateAsTopLevelExp(expSyntax, hintType, code);
}
