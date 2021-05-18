using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Text;
using System.Linq;
using Gum.Infra;
using Pretune;

namespace Gum.Syntax
{
    [AutoConstructor, ImplementIEquatable]
    public partial struct FuncParamInfo
    {
        public ImmutableArray<TypeAndName> Parameters { get; }
        public int? VariadicParamIndex { get; }
    }
}
