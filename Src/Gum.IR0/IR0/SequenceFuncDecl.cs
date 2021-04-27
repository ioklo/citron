using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Text;
using Pretune;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial class SequenceFuncDecl : IDecl
    {
        public Name Name { get; }
        public bool IsThisCall { get; }
        public Type YieldType { get; }
        public ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<ParamInfo> ParamInfos { get; }
        public Stmt Body { get; }
    }
}
