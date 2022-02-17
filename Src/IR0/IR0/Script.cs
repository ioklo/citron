using Pretune;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Citron.Collections;
using Citron.CompileTime;

namespace Citron.IR0
{
    // DeclSymbolId -> Stmt
    [AutoConstructor]
    public partial struct IR0StmtBody
    {
        public DeclSymbolPath Path { get; }
        public Stmt Body { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class Script
    {
        public Name Name { get; }
        public ImmutableArray<IR0StmtBody> Bodies { get; }
        public ImmutableArray<Stmt> TopLevelStmts { get; }
    }
}
