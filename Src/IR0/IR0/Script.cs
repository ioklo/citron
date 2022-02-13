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
        public DeclSymbolId Id { get; }
        public Stmt Body { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class Script
    {
        public ModuleName Name { get; }
        public ImmutableArray<IR0StmtBody> Bodies { get; }
        
        //public ImmutableArray<TypeDecl> GlobalTypeDecls { get; }
        //public ImmutableArray<FuncDecl> GlobalFuncDecls { get; }
        //public ImmutableArray<CallableMemberDecl> CallableMemberDecls { get; }
        //public ImmutableArray<Stmt> TopLevelStmts { get; }
    }
}
