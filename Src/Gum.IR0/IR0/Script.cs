using Pretune;
using System;
using System.Collections;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;
using System.Text;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial class Script
    {
        public ModuleName Name { get; }
        
        public ImmutableArray<TypeDecl> GlobalTypeDecls { get; }
        public ImmutableArray<FuncDecl> GlobalFuncDecls { get; }
        public ImmutableArray<CallableMemberDecl> CallableMemberDecls { get; }
        public ImmutableArray<Stmt> TopLevelStmts { get; }
    }
}
