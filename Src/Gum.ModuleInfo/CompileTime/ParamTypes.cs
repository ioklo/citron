using Gum.Collections;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.CompileTime
{
    [AutoConstructor, ImplementIEquatable]
    public partial struct ParamKindAndType
    {
        public ParamKind Kind { get; }
        public TypeId Type { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial struct ParamTypes
    {
        public bool IsEmpty => items.Length == 0;
        public int Length => items.Length;
        public ParamKindAndType this[int index] => items[index];        

        ImmutableArray<ParamKindAndType> items;
    }
}
