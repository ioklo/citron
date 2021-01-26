using Pretune;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Gum.CompileTime
{
    [AutoConstructor]
    public partial class ModuleInfo
    {
        public ModuleName Name { get; }
        public ImmutableArray<NamespaceInfo> Namespaces { get; }
        public ImmutableArray<TypeInfo> Types { get; }
        public ImmutableArray<FuncInfo> Funcs { get; }
    }
}
