using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    static class MTypes
    {
        public static M.Type Int { get; } = new M.GlobalType("System.Runtime", new M.NamespacePath("System"), "Int32", ImmutableArray<M.Type>.Empty);
        public static M.Type Bool { get; } = new M.GlobalType("System.Runtime", new M.NamespacePath("System"), "Boolean", ImmutableArray<M.Type>.Empty);
        public static M.Type String { get; } = new M.GlobalType("System.Runtime", new M.NamespacePath("System"), "String", ImmutableArray<M.Type>.Empty);
    }
}
