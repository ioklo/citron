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
            scopeContext.AddLocalVarInfo(parameter.Type, parameter.Name);
        }

        var stmtVisitor = new StmtVisitor(scopeContext);
        var bodyStmts = stmtVisitor.VisitBody(body);
        
        context.AddBody(symbol, bodyStmts);
    }

    public void VisitClassConstructorDecl(ImmutableArray<Stmt> body, ClassConstructorDeclSymbol symbol)
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
        var bodyStmts = stmtVisitor.VisitBody(body);
        
        context.AddBody(symbol, bodyStmts);
    }
}
