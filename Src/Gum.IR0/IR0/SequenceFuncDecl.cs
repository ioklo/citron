using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Text;
using Pretune;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial class SequenceFuncDecl : Decl
    {
        public ImmutableArray<Decl> Decls { get; }

        public Name Name { get; }
        public bool IsThisCall { get; }
        public Path YieldType { get; }
        public ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<ParamInfo> ParamInfos { get; }
        public Stmt Body { get; }
    }
}
