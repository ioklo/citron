using Gum.Infra;
using Pretune;
using System;
using System.Collections.Generic;

namespace Gum.IR0
{
    // int a
    [AutoConstructor, ImplementIEquatable]
    public partial struct TypeAndName : IPure
    {
        public Path Type { get; }
        public string Name { get; }

        public void EnsurePure()
        {
            Misc.EnsurePure(Type);
        }

        public void Deconstruct(out Path outType, out string outName)
        {
            outType = Type;
            outName = Name;
        }
    }
}