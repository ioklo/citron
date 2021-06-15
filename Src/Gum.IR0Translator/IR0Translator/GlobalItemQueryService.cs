using M = Gum.CompileTime;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Gum.IR0Translator
{   
    // TODO: using cache after mature    
    struct GlobalItemQueryService
    {
        M.ModuleInfo moduleInfo;

        public static M.ItemInfo? GetGlobalItem(M.ModuleInfo moduleInfo, M.NamespacePath path, ItemPathEntry entry)
        {
            var service = new GlobalItemQueryService(moduleInfo);
            return service.GetGlobalItemCore(path, entry);
        }

        public static IEnumerable<M.FuncInfo> GetGlobalFuncs(M.ModuleInfo moduleInfo, M.NamespacePath path, M.Name funcName)
        {
            var service = new GlobalItemQueryService(moduleInfo);
            return service.GetGlobalFuncsCore(path, funcName);
        }

        GlobalItemQueryService(M.ModuleInfo moduleInfo)
        {
            this.moduleInfo = moduleInfo;
        }        
        
        M.ItemInfo? GetChildItem(ImmutableArray<M.TypeInfo> types, ImmutableArray<M.FuncInfo> funcs, ItemPathEntry entry)
        {
            // paramHash가 있으면 함수에서만 검색
            if (entry.ParamHash.Length == 0)
            {
                foreach(var type in types)
                    if (type.TypeParams.Length == entry.TypeParamCount &&
                        type.Name.Equals(entry.Name))
                        return type;

                foreach (var func in funcs)
                    if (func.TypeParams.Length == entry.TypeParamCount &&
                        func.Parameters.Length == 0 &&
                        func.Name.Equals(entry.Name))
                        return func;
            }
            else
            {
                foreach (var func in funcs)
                    if (func.TypeParams.Length == entry.TypeParamCount &&
                        func.Name.Equals(entry.Name))
                    {
                        // TODO: 매번 계산한다
                        var paramHash = Misc.MakeParamHash(ImmutableArray.CreateRange(func.Parameters, param => (param.Kind, param.Type)));
                        if (paramHash == entry.ParamHash)
                            return func;
                    }
            }

            return null;
        }

        M.ItemInfo? GetGlobalItemCore(M.NamespacePath path, ItemPathEntry entry)
        {
            if (path.IsRoot)
                return GetChildItem(moduleInfo.Types, moduleInfo.Funcs, entry);

            var ns = moduleInfo.GetNamespace(path);
            if (ns == null)
                return null;
            
            return GetChildItem(ns.Types, ns.Funcs, entry);
        }

        IEnumerable<M.FuncInfo> GetGlobalFuncsCore(M.NamespacePath path, M.Name funcName)
        {
            IEnumerable<M.FuncInfo> GetChildFuncs(ImmutableArray<M.FuncInfo> funcs)
                => funcs.Where(func => func.Name.Equals(funcName));

            if (path.IsRoot)
                return GetChildFuncs(moduleInfo.Funcs);

            var curNamespace = moduleInfo.GetNamespace(path);

            if (curNamespace == null)
                return Enumerable.Empty<M.FuncInfo>();
                
            return GetChildFuncs(curNamespace.Funcs);
        }
    }
}