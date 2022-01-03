using M = Gum.CompileTime;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Gum.Analysis
{   
    // TODO: using cache after mature    
    // 어디서 쓰이는가: TypeInfoRepository, TypeExpEvaluator, RootAnalyzer
    public struct GlobalItemQueryService
    {
        IModuleDecl moduleInfo;

        public static IModuleItemDecl? GetGlobalItem(IModuleDecl moduleInfo, M.ItemPathEntry entry)
        {
            var service = new GlobalItemQueryService(moduleInfo);
            return service.GetGlobalItemCore(entry);
        }

        GlobalItemQueryService(IModuleDecl moduleInfo)
        {
            this.moduleInfo = moduleInfo;
        }        
        
        IModuleItemDecl? GetChildItem(IModuleTypeContainer types, IModuleFuncContainer funcs, M.ItemPathEntry entry)
        {
            // paramHash가 있으면 함수에서만 검색
            if (entry.ParamTypes.IsEmpty)
            {
                var type = types.GetType(entry.Name, entry.TypeParamCount);
                if (type != null) return type;
            }

            return funcs.GetFunc(entry.Name, entry.TypeParamCount, entry.ParamTypes);
        }

        IModuleItemDecl? GetGlobalItemCore(M.ItemPathEntry entry)
        {
            if (path.IsRoot)
                return GetChildItem(moduleInfo, moduleInfo, entry);

            var ns = moduleInfo.GetNamespace(path);
            if (ns == null)
                return null;
            
            return GetChildItem(ns, ns, entry);
        }
    }
}