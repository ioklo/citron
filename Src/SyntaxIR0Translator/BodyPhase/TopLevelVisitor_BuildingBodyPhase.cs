using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Citron.Collections;
using Citron.Symbol;
using Citron.Syntax;
using R = Citron.IR0;

namespace Citron.Analysis;

struct TopLevelVisitor_BuildingBodyPhase
{
    BuildingBodyPhaseContext context;

    public TopLevelVisitor_BuildingBodyPhase(BuildingBodyPhaseContext context)
    {
        this.context = context;
    }

    public void VisitGlobalFuncDecl(ImmutableArray<Stmt> body, GlobalFuncDeclSymbol symbol, bool bSeqFunc)
    {
        var scopeContext = context.MakeNewScopeContext(symbol, bSeqFunc: bSeqFunc, symbol.GetReturn());

        // 파라미터를 로컬로 추가
        int paramCount = symbol.GetParameterCount();
        for(int i = 0; i < paramCount; i++)
        {
            var parameter = symbol.GetParameter(i);
            var bRef = (parameter.Kind == FuncParameterKind.Ref);
            scopeContext.AddLocalVarInfo(bRef, parameter.Type, parameter.Name);
        }

        var stmtVisitor = new StmtVisitor(scopeContext);

        // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다 => 분석기에서 해야 한다. 여기서는 Translation만 한다
        stmtVisitor.VisitBody(body);

        var rstmts = scopeContext.MakeStmts();
        context.AddBody(symbol, rstmts);
    }
}
