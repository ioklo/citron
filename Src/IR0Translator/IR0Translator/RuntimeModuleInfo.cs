using Citron.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Citron.Infra.Misc;

namespace Citron.IR0Translator
{   
    internal static class RuntimeModuleDecl // use internal for tests
    {
        public static readonly ModuleDecl Instance = Create();

        static ModuleDecl Create()
        {
            var intType = new GlobalTypeDecl(Accessor.Public, new StructDecl(new Name.Normal("Int32"), default, null, default, default, default, default, default));
            var boolType = new GlobalTypeDecl(Accessor.Public, new StructDecl(new Name.Normal("Boolean"), default, null, default, default, default, default, default));
            var stringType = new GlobalTypeDecl(Accessor.Public, new StructDecl(new Name.Normal("String"), default, null, default, default, default, default, default));
            var types = Arr(intType, boolType, stringType);
            
            var systemNS = new NamespaceDecl(new Name.Normal("System"), default, types, default);

            return new ModuleDecl(new Name.Normal("System.Runtime"), Arr(systemNS), default, default);
        }
    }
}
