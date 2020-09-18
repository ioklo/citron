using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Gum.CompileTime
{
    public class ModuleInfoService
    {
        IEnumerable<IModuleInfo> moduleInfos;

        public ModuleInfoService(IEnumerable<IModuleInfo> moduleInfos)
        {
            this.moduleInfos = moduleInfos;
        }
        
        public IEnumerable<ITypeInfo> GetTypeInfos(ModuleItemId typeId)
        {
            foreach (var moduleInfo in moduleInfos)
            {
                if (moduleInfo.GetTypeInfo(typeId, out var typeInfo))
                {
                    yield return typeInfo;
                }
            }
        }
        
        public IEnumerable<FuncInfo> GetFuncInfos(ModuleItemId funcId)
        {
            foreach(var moduleInfo in moduleInfos)
            {
                if( moduleInfo.GetFuncInfo(funcId, out var funcInfo))
                {
                    yield return funcInfo;
                }
            }
        }

        public IEnumerable<VarInfo> GetVarInfos(ModuleItemId varId)
        {
            foreach(var moduleInfo in moduleInfos)
            {
                if (moduleInfo.GetVarInfo(varId, out var varInfo))
                {
                    yield return varInfo;
                }
            }
        }
    }
}
