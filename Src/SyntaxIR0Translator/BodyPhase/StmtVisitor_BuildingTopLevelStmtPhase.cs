using System;
using S = Citron.Syntax;

namespace Citron.Analysis;

class StmtVisitor_BuildingTopLevelStmtPhase
{
    BuildingTopLevelStmtPhaseContext context;

    public StmtVisitor_BuildingTopLevelStmtPhase(BuildingTopLevelStmtPhaseContext context)
    {
        this.context = context;
    }

    public void Visit(S.Stmt stmt)
    {
        
    }
}