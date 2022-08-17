using Pretune;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Citron.Collections;
using Citron.Module;
using Citron.Symbol;

namespace Citron.IR0
{
    // DeclSymbolId -> Stmt
    [AutoConstructor]
    public partial struct StmtBody
    {
        public DeclSymbolPath Path { get; }
        public ImmutableArray<Stmt> Stmts { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class Script
    {
        public ModuleDeclSymbol ModuleDeclSymbol { get; }
        public ImmutableArray<StmtBody> StmtBodies { get; }
    }
}
