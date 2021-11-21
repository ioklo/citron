using Gum.Analysis;
using Gum.Collections;
using Gum.Infra;
using System;
using System.Collections.Generic;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public class InternalModuleInfo : IPure, IModuleInfo
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

        IModuleNamespaceInfo? IModuleNamespaceContainer.GetNamespace(M.Name name)
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

        IModuleItemInfo? IModuleInfo.GetItem(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            var candidates = new Candidates<IModuleItemInfo>();

            // TODO:
            //if (typeParamCount == 0 && paramTypes.IsEmpty)
            //{
            //    var ns = namespaceDict.Get(name);
            //    if (ns != null)
            //        candidates.Add(ns);
            //}

            if (paramTypes.IsEmpty)
            {
                var type = typeDict.Get(name, typeParamCount);
                if (type != null)
                    candidates.Add(type);
            }

            var func = funcDict.Get(name, typeParamCount, paramTypes);
            if (func != null)
                candidates.Add(func);

            return candidates.GetSingle(); // TODO: 에러처리
        }
    }
}