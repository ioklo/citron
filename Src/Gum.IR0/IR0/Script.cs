using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

using Type = Gum.IR0.Type;

namespace Gum.IR0
{
    public partial class Script
    {
        public ImmutableArray<TypeDecl> TypeDecls { get; } // class struct enum interface
        public ImmutableArray<FuncDecl> FuncDecls { get; } // 
        public ImmutableArray<Stmt> TopLevelStmts { get; } 

        public Script(
            IEnumerable<TypeDecl> typeDecls,
            IEnumerable<FuncDecl> funcDecls,
            IEnumerable<Stmt> topLevelStmts)
        {
            TypeDecls = typeDecls.ToImmutableArray();
            FuncDecls = funcDecls.ToImmutableArray();

            TopLevelStmts = topLevelStmts.ToImmutableArray();
        }
    }
}
