using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    static class MTypes
    {
        public static M.Type Int { get; } = new M.GlobalType("System.Runtime", new M.NamespacePath(new M.Name.Normal("System")), new M.Name.Normal("Int32"), ImmutableArray<M.Type>.Empty);
        public static M.Type Bool { get; } = new M.GlobalType("System.Runtime", new M.NamespacePath(new M.Name.Normal("System")), new M.Name.Normal("Boolean"), ImmutableArray<M.Type>.Empty);
        public static M.Type String { get; } = new M.GlobalType("System.Runtime", new M.NamespacePath(new M.Name.Normal("System")), new M.Name.Normal("String"), ImmutableArray<M.Type>.Empty);
    }
}
