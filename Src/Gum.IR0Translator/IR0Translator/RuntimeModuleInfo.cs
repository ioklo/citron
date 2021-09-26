using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gum.Infra.Misc;

namespace Gum.IR0Translator
{   
    internal static class RuntimeModuleInfo // use internal for tests
    {
        public static readonly ModuleInfo Instance = Create();

        static ModuleInfo Create()
        {
            var intType = new StructInfo(new Name.Normal("Int32"), default, null, default, default, default, default, default);
            var boolType = new StructInfo(new Name.Normal("Boolean"), default, null, default, default, default, default, default);
            var stringType = new StructInfo(new Name.Normal("String"), default, null, default, default, default, default, default);
            var types = Arr<TypeInfo>(intType, boolType, stringType);
            
            var systemNS = new NamespaceInfo("System", default, types, default);

            return new ModuleInfo("System.Runtime", Arr(systemNS), default, default);
        }
    }
}
