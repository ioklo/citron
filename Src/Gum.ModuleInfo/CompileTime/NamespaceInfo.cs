using System;
using System.Collections.Generic;
using System.Text;
using Gum.Collections;
using Pretune;

namespace Gum.CompileTime
{
    // 이름 없음.
    [AutoConstructor]
    public partial class NamespaceInfo
    {
        public Name Name { get; }

        public ImmutableArray<NamespaceInfo> Namespaces { get; }
        public ImmutableArray<TypeInfo> Types { get; }
        public ImmutableArray<FuncInfo> Funcs { get; }
    }
}
