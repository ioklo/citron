using Pretune;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Gum.Infra;

namespace Gum.CompileTime
{
    [AutoConstructor]
    public partial class ModuleInfo : IPure
    {
        public ModuleName Name { get; }
        public ImmutableArray<NamespaceInfo> Namespaces { get; }
        public ImmutableArray<TypeInfo> Types { get; }
        public ImmutableArray<FuncInfo> Funcs { get; }

        public void EnsurePure()
        {
        }
    }
}
