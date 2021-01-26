using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Immutable;
using Gum.Misc;

namespace Gum.CompileTime
{
    // 이름 없음.
    public class NamespaceInfo
    {
        public NamespaceName Name { get; }

        public ImmutableArray<NamespaceInfo> Namespaces { get; }
        public ImmutableArray<TypeInfo> Types { get; }
        public ImmutableArray<FuncInfo> Funcs { get; }
    }
}
