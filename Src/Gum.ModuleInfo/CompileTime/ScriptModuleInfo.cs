using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Gum.CompileTime
{
    public class ScriptModuleInfo : IModuleInfo
    {
        public string ModuleName { get; }

        private ImmutableDictionary<ModuleItemId, ITypeInfo> typeInfos;
        private ImmutableDictionary<ModuleItemId, FuncInfo> funcInfos;
        private ImmutableDictionary<ModuleItemId, VarInfo> varInfos;

        public ScriptModuleInfo(string moduleName, IEnumerable<ITypeInfo> typeInfos, IEnumerable<FuncInfo> funcInfos, IEnumerable<VarInfo> varInfos)
        {
            ModuleName = moduleName;

            this.typeInfos = typeInfos.ToImmutableDictionary(typeInfo => typeInfo.TypeId);
            this.funcInfos = funcInfos.ToImmutableDictionary(funcInfo => funcInfo.FuncId);
            this.varInfos = varInfos.ToImmutableDictionary(varInfo => varInfo.VarId);
        }

        public bool GetFuncInfo(ModuleItemId id, [NotNullWhen(true)] out FuncInfo? funcInfo)
        {
            return funcInfos.TryGetValue(id, out funcInfo);
        }

        public bool GetTypeInfo(ModuleItemId id, [NotNullWhen(true)] out ITypeInfo? typeInfo)
        {
            return typeInfos.TryGetValue(id, out typeInfo);
        }

        public bool GetVarInfo(ModuleItemId id, [NotNullWhen(true)] out VarInfo? varInfo)
        {
            return varInfos.TryGetValue(id, out varInfo);
        }
    }
}