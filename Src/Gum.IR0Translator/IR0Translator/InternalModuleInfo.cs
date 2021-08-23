using Gum.Collections;
using Gum.Infra;
using System;
using System.Collections.Generic;
using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    partial class InternalModuleInfo : IPure, IModuleInfo
    {
        M.ModuleName moduleName;

        ModuleTypeDict typeDict;
        ModuleFuncDict funcDict;

        public InternalModuleInfo(M.ModuleName moduleName, ImmutableArray<IModuleTypeInfo> types, ImmutableArray<IModuleFuncInfo> funcs)
        {
            this.moduleName = moduleName;
            this.typeDict = new ModuleTypeDict(types);
            this.funcDict = new ModuleFuncDict(funcs);
        }

        public void EnsurePure()
        {
        }

        public M.ModuleName GetName()
        {
            return moduleName;
        }

        M.ModuleName IModuleInfo.GetName()
        {
            return moduleName;
        }

        IModuleNamespaceInfo? IModuleNamespaceContainer.GetNamespace(M.NamespaceName name)
        {
            throw new NotImplementedException();
        }

        ImmutableArray<IModuleFuncInfo> IModuleFuncContainer.GetFuncs(M.Name name, int minTypeParamCount)
        {
            return funcDict.Get(name, minTypeParamCount);
        }

        // 문제는
        IModuleTypeInfo? IModuleTypeContainer.GetType(M.Name name, int typeParamCount)
        {
            return typeDict.Get(name, typeParamCount);
        }

        IModuleFuncInfo? IModuleFuncContainer.GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return funcDict.Get(name, typeParamCount, paramTypes);
        }
    }
}