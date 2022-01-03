using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M = Gum.CompileTime;
using static Gum.CompileTime.ItemPathExtensions;

namespace Gum.IR0Translator
{
    static class MTypes
    {
        public static readonly M.Name System_Runtime = new M.Name.Normal("System.Runtime");
        public static readonly M.NamespacePath System = new M.NamespacePath(null, new M.Name.Normal("System"));

        public static readonly M.TypeId Int = new M.RootTypeId(System_Runtime, System, new M.Name.Normal("Int32"), default);
        public static readonly M.TypeId Bool = new M.RootTypeId(System_Runtime, System, new M.Name.Normal("Boolean"), default);
        public static readonly M.TypeId String = new M.RootTypeId(System_Runtime, System, new M.Name.Normal("String"), default);
    }
}
