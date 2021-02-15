using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Gum.IR0
{
    public partial class Script
    {
        public ImmutableArray<TypeDecl> TypeDecls { get; } // class struct enum interface
        public ImmutableArray<FuncDecl> FuncDecls { get; } // normal / sequence
        public ImmutableArray<Stmt> TopLevelStmts { get; } 

        public Script(
            ImmutableArray<TypeDecl> typeDecls,
            ImmutableArray<FuncDecl> funcDecls,
            ImmutableArray<Stmt> topLevelStmts)
        {
            TypeDecls = typeDecls;
            FuncDecls = funcDecls;
            TopLevelStmts = topLevelStmts;
        }
    }
}
