using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;
using System.Text;
using Pretune;

namespace Gum.CompileTime
{
    [AutoConstructor, ImplementIEquatable]
    public partial class FuncInfo : ItemInfo
    {
        public override Name Name { get; }
        public bool IsSequenceFunc { get; }
        public bool IsInstanceFunc { get; }
        public ImmutableArray<string> TypeParams { get; }
        public Type RetType { get; }
        public ParamInfo ParamInfo { get; }
    }
}
