﻿using M = Gum.CompileTime;
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
        IModuleInfo moduleInfo;

        public static IModuleItemInfo? GetGlobalItem(IModuleInfo moduleInfo, M.NamespacePath path, M.ItemPathEntry entry)
        {
            var service = new GlobalItemQueryService(moduleInfo);
            return service.GetGlobalItemCore(path, entry);
        }

        GlobalItemQueryService(IModuleInfo moduleInfo)
        {
            this.moduleInfo = moduleInfo;
        }        
        
        IModuleItemInfo? GetChildItem(IModuleTypeContainer types, IModuleFuncContainer funcs, M.ItemPathEntry entry)
        {
            // paramHash가 있으면 함수에서만 검색
            if (entry.ParamTypes.IsEmpty)
            {
                var type = types.GetType(entry.Name, entry.TypeParamCount);
                if (type != null) return type;
            }

            return funcs.GetFunc(entry.Name, entry.TypeParamCount, entry.ParamTypes);
        }

        IModuleItemInfo? GetGlobalItemCore(M.NamespacePath path, M.ItemPathEntry entry)
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