using Citron.Collections;
using Citron.Symbol;
using Citron.Syntax;
using Pretune;

namespace Citron.Analysis;

[AutoConstructor]
partial struct StructVisitor_BuildingBodyPhase
{
    BuildingBodyPhaseContext context;

    public void VisitStructMemberFuncDecl(ImmutableArray<Stmt> body, StructMemberFuncDeclSymbol symbol, bool bSeqFunc)
    {
        var scopeContext = context.MakeNewScopeContext(symbol, bSeqFunc: bSeqFunc, symbol.GetReturn());

        // 파라미터를 로컬로 추가
        int paramCount = symbol.GetParameterCount();
        for (int i = 0; i < paramCount; i++)
        {
            var parameter = symbol.GetParameter(i);
            scopeContext.AddLocalVarInfo(parameter.Type, parameter.Name);
        }

        var stmtVisitor = new StmtVisitor(scopeContext);
        stmtVisitor.VisitBody(body);

        var rstmts = scopeContext.MakeStmts();
        context.AddBody(symbol, rstmts);
    }

    public void VisitStructConstructorDecl(ImmutableArray<Stmt> body, StructConstructorDeclSymbol symbol)
    {
        var scopeContext = context.MakeNewScopeContext(symbol, bSeqFunc: false, funcReturn: null);

        // 파라미터를 로컬로 추가
        int paramCount = symbol.GetParameterCount();
        for (int i = 0; i < paramCount; i++)
        {
            var parameter = symbol.GetParameter(i);
            scopeContext.AddLocalVarInfo(parameter.Type, parameter.Name);
        }

        var stmtVisitor = new StmtVisitor(scopeContext);
        stmtVisitor.VisitBody(body);

        var rstmts = scopeContext.MakeStmts();
        context.AddBody(symbol, rstmts);
    }
}
