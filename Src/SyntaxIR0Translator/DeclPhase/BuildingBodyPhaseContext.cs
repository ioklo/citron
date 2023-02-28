using Citron.Collections;
using Citron.IR0;
using Citron.Symbol;
using System;

namespace Citron.Analysis
{
    class BuildingBodyPhaseContext
    {
        GlobalContext globalContext;

        public BuildingBodyPhaseContext(GlobalContext globalContext)
        {
            this.globalContext = globalContext;
        }

        public ScopeContext MakeNewScopeContext(IFuncDeclSymbol symbol, bool bSeqFunc, FuncReturn? funcReturn)
        {
            return globalContext.MakeNewScopeContext(symbol, bSeqFunc, funcReturn);
        }

        public void AddBody(IFuncDeclSymbol symbol, ImmutableArray<Stmt> body)
        {
            globalContext.AddBody(symbol, body);
        }
    }
}