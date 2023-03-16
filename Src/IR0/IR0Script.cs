using Pretune;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Citron.Collections;
using Citron.Symbol;
using Citron.Infra;

namespace Citron.IR0
{
    // DeclSymbol -> Stmt
    public record struct StmtBody(IDeclSymbolNode DSymbol, ImmutableArray<Stmt> Stmts) : ICyclicEqualityComparableStruct<StmtBody>
    {
        bool ICyclicEqualityComparableStruct<StmtBody>.CyclicEquals(ref StmtBody other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(DSymbol, other.DSymbol))
                return false;

            if (!Stmts.Equals(other.Stmts))
                return false;

            return true;
        }
    }

    public record class Script(ModuleDeclSymbol ModuleDeclSymbol, ImmutableArray<StmtBody> StmtBodies) : ICyclicEqualityComparableClass<Script>
    {
        bool ICyclicEqualityComparableClass<Script>.CyclicEquals(Script other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(ModuleDeclSymbol, other.ModuleDeclSymbol))
                return false;

            var thisBodies = this.StmtBodies;
            var otherBodies = other.StmtBodies;
            if (!thisBodies.CyclicEqualsStructItem(ref otherBodies, ref context))
                return false;

            return true;
        }
    }
}
