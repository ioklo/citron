using System;

namespace Citron.Analysis
{
    class BuildingTopLevelStmtPhaseContext
    {
        ScopeContext context;

        public BuildingTopLevelStmtPhaseContext(ScopeContext context)
        {
            this.context = context;
        }

        public ScopeContext GetScopeContext()
        {
            return context;
        }
    }
}