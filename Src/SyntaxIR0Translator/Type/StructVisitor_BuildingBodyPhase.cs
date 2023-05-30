using Citron.Collections;
using Citron.Symbol;
using Citron.Syntax;
using Pretune;

namespace Citron.Analysis;

struct StructVisitor_BuildingBodyPhase
{
    public static bool VisitStructMemberFuncDecl(ImmutableArray<Stmt> body, BuildingBodyPhaseContext context, StructMemberFuncDeclSymbol symbol, bool bSeqFunc)
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

    public static bool VisitStructConstructorDecl(ImmutableArray<Stmt> body, BuildingBodyPhaseContext context, StructConstructorDeclSymbol symbol)
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
