using Gum.Infra;
using Pretune;
using System;
using System.Collections.Generic;

namespace Gum.IR0
{
    public enum ParamKind
    {
        Default,
        Params,
        Ref,
    }

    // int a
    [AutoConstructor, ImplementIEquatable]
    public partial struct Param 
    {
        public ParamKind Kind { get; }
        public Path Type { get; }
        public Name Name { get; }
    }
}