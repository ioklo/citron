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
        public ImmutableArray<Type> Types { get; }
        public ImmutableArray<Func> Funcs { get; }
        public ImmutableArray<SeqFunc> SeqFuncs { get; }

        public ImmutableArray<Stmt> TopLevelStmts { get; }

        public Script(IEnumerable<Type> types, IEnumerable<Func> funcs, IEnumerable<SeqFunc> seqFuncs, IEnumerable<Stmt> topLevelStmts)
        {            
            Types = types.ToImmutableArray();

            Funcs = funcs.ToImmutableArray();
            SeqFuncs = seqFuncs.ToImmutableArray();

            TopLevelStmts = topLevelStmts.ToImmutableArray();
        }
    }
}
