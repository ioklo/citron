using Gum.CompileTime;
using Gum.Misc;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Gum.IR0
{   
    // TODO: using cache after mature    
    struct GlobalItemQueryService
    {
        ModuleInfo moduleInfo;

        public static ItemInfo? GetGlobalItem(ModuleInfo moduleInfo, NamespacePath path, ItemPathEntry entry)
        {
            var service = new GlobalItemQueryService(moduleInfo);
            return service.GetGlobalItemCore(path, entry);
        }

        public IEnumerable<FuncInfo> GetGlobalFuncs(NamespacePath path, Name funcName)
        {
            var service = new GlobalItemQueryService(moduleInfo);
            return service.GetGlobalFuncsCore(path, funcName);
        }

        GlobalItemQueryService(ModuleInfo moduleInfo)
        {
            this.moduleInfo = moduleInfo;
        }
        
        NamespaceInfo? GetNamespace(NamespacePath path)
        {
            Debug.Assert(!path.IsRoot);

            NamespaceInfo? curNamespace = null;
            foreach (var entry in path.Entries)
            {
                if (curNamespace == null)
                {   
                    curNamespace = moduleInfo.Namespaces.FirstOrDefault(info => info.Name.Equals(entry));
                    if (curNamespace == null)
                        return null;
                }
                else
                {
                    curNamespace = curNamespace.Namespaces.FirstOrDefault(info => info.Name.Equals(entry));
                    if (curNamespace == null)
                        return null;
                }
            }

            return curNamespace;
        }

        ItemInfo? GetChildItem(ImmutableArray<TypeInfo> types, ImmutableArray<FuncInfo> funcs, ItemPathEntry entry)
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
                        func.ParamTypes.Length == 0 &&
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
                        var paramHash = Misc.MakeParamHash(func.ParamTypes);
                        if (paramHash == entry.ParamHash)
                            return func;
                    }
            }

            return null;
        }

        ItemInfo? GetGlobalItemCore(NamespacePath path, ItemPathEntry entry)
        {
            if (path.IsRoot)
                return GetChildItem(moduleInfo.Types, moduleInfo.Funcs, entry);

            var ns = GetNamespace(path);
            if (ns == null)
                return null;
            
            return GetChildItem(ns.Types, ns.Funcs, entry);
        }

        IEnumerable<FuncInfo> GetGlobalFuncsCore(NamespacePath path, Name funcName)
        {
            IEnumerable<FuncInfo> GetChildFuncs(ImmutableArray<FuncInfo> funcs)
                => funcs.Where(func => func.Name.Equals(funcName));

            if (path.IsRoot)
                return GetChildFuncs(moduleInfo.Funcs);

            var curNamespace = GetNamespace(path);

            if (curNamespace == null)
                return Enumerable.Empty<FuncInfo>();
                
            return GetChildFuncs(curNamespace.Funcs);
        }
    }
}