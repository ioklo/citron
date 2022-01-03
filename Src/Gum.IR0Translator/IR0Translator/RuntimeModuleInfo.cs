using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gum.Infra.Misc;

namespace Gum.IR0Translator
{   
    internal static class RuntimeModuleDecl // use internal for tests
    {
        public static readonly ModuleDecl Instance = Create();

        static ModuleDecl Create()
        {
            var intType = new StructDecl(new Name.Normal("Int32"), default, null, default, default, default, default, default);
            var boolType = new StructDecl(new Name.Normal("Boolean"), default, null, default, default, default, default, default);
            var stringType = new StructDecl(new Name.Normal("String"), default, null, default, default, default, default, default);
            var types = Arr<TypeDecl>(intType, boolType, stringType);
            
            var systemNS = new NamespaceDecl("System", default, types, default);

            return new ModuleDecl("System.Runtime", Arr(systemNS), default, default);
        }
    }
}
