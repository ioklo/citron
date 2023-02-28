using Citron.Collections;
using Citron.Symbol;
using Citron.Syntax;
using Pretune;
using System;

namespace Citron.Analysis;

[AutoConstructor]
partial struct ClassVisitor_BuilindBodyPhase
{
    BuildingBodyPhaseContext context;

    public void VisitClassFuncDecl(ImmutableArray<Stmt> body, ClassMemberFuncDeclSymbol symbol, bool bSeqFunc)
    {   
        var scopeContext = context.MakeNewScopeContext(symbol, bSeqFunc: bSeqFunc, symbol.GetReturn());

        // 파라미터를 로컬로 추가
        int paramCount = symbol.GetParameterCount();
        for (int i = 0; i < paramCount; i++)
        {
            var parameter = symbol.GetParameter(i);
            var bRef = (parameter.Kind == FuncParameterKind.Ref);
            scopeContext.AddLocalVarInfo(bRef, parameter.Type, parameter.Name);
        }

        var stmtVisitor = new StmtVisitor(scopeContext);
        stmtVisitor.VisitBody(body);

        var rstmts = scopeContext.MakeStmts();
        context.AddBody(symbol, rstmts);
    }

    public void VisitClassConstructorDecl(ImmutableArray<Stmt> body, ClassConstructorDeclSymbol symbol)
    {
        var scopeContext = context.MakeNewScopeContext(symbol, bSeqFunc: false, funcReturn: null);

        // 파라미터를 로컬로 추가
        int paramCount = symbol.GetParameterCount();
        for (int i = 0; i < paramCount; i++)
        {
            var parameter = symbol.GetParameter(i);
            var bRef = (parameter.Kind == FuncParameterKind.Ref);
            scopeContext.AddLocalVarInfo(bRef, parameter.Type, parameter.Name);
        }

        var stmtVisitor = new StmtVisitor(scopeContext);
        stmtVisitor.VisitBody(body);

        var rstmts = scopeContext.MakeStmts();
        context.AddBody(symbol, rstmts);
    }
}
