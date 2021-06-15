using Gum.Infra;
using Pretune;
using System;
using System.Collections.Generic;

namespace Gum.IR0
{
    public enum ParamKind
    {
        Normal,
        Params,
        Ref,
    }

    // int a
    [AutoConstructor, ImplementIEquatable]
    public partial struct Param : IPure
    {
        public ParamKind Kind { get; }
        public Path Type { get; }
        public string Name { get; }

        public void EnsurePure()
        {
            Misc.EnsurePure(Type);
        }

        public void Deconstruct(out ParamKind outKind, out Path outType, out string outName)
        {
            outKind = Kind;
            outType = Type;
            outName = Name;
        }
    }
}