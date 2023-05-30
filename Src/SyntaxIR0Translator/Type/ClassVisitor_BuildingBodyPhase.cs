using Citron.Collections;
using Citron.Symbol;
using Citron.Syntax;
using Pretune;
using System;

namespace Citron.Analysis;

struct ClassVisitor_BuildingBodyPhase
{
    public static bool VisitClassFuncDecl(ImmutableArray<Stmt> body, BuildingBodyPhaseContext context, ClassMemberFuncDeclSymbol symbol, bool bSeqFunc)
    {   
        var scopeContext = context.MakeNewScopeContext(symbol, bSeqFunc: bSeqFunc, symbol.GetReturn());

        // 파라미터를 로컬로 추가
        int paramCount = symbol.GetParameterCount();
        for (int i = 0; i < paramCount; i++)
        {
            var parameter = symbol.GetParameter(i);
            scopeContext.AddLocalVarInfo(parameter.Type, parameter.Name);
        }
        
        var bodyStmtsResult = StmtVisitor.TranslateBody(body, scopeContext);
        if (!bodyStmtsResult.IsValid(out var bodyStmts))
            return false;
        
        context.AddBody(symbol, bodyStmts);
        return true;
    }

    public static bool VisitClassConstructorDecl(ImmutableArray<Stmt> body, BuildingBodyPhaseContext context, ClassConstructorDeclSymbol symbol)
    {
        var scopeContext = context.MakeNewScopeContext(symbol, bSeqFunc: false, funcReturn: null);

        // 파라미터를 로컬로 추가
        int paramCount = symbol.GetParameterCount();
        for (int i = 0; i < paramCount; i++)
        {
            var parameter = symbol.GetParameter(i);            
            scopeContext.AddLocalVarInfo(parameter.Type, parameter.Name);
        }
        
        var bodyStmtsResult = StmtVisitor.TranslateBody(body, scopeContext);
        if (!bodyStmtsResult.IsValid(out var bodyStmts))
            return false;
        
        context.AddBody(symbol, bodyStmts);
        return true;
    }
}
