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
    // Skeleton 다음 단계
    class InternalModuleInfo
    {
        // 최상위
        Dictionary<NamespaceId, NamespaceInfo> globalNamespacesById;
        Dictionary<ItemPathEntry, ItemInfo> globalItemsByElem;
        MultiDict<Name, FuncInfo> globalFuncsByName;

        public InternalModuleInfo(IEnumerable<NamespaceInfo> globalNamespaces, IEnumerable<ItemInfo> globalItems)
        {
            this.globalNamespacesById = globalNamespaces.ToDictionary(ns => ns.Id);
            this.globalItemsByElem = new Dictionary<ItemPathEntry, ItemInfo>();
            this.globalFuncsByName = new MultiDict<Name, FuncInfo>();

            foreach(var item in globalItems)
            {
                Debug.Assert(IsGlobalItem(item));

                globalItemsByElem.Add(item.GetLocalId(), item);

                if (item is FuncInfo func)
                    globalFuncsByName.Add(item.GetLocalId().Name, func);
                else
                    throw new InvalidOperationException();
            }
        }

        bool IsGlobalItem(ItemInfo item)
        {
            return item.GetId().OuterEntries.Length == 0;
        }

        public ModuleName GetModuleName()
        { 
            return ModuleName.Internal;
        }

        NamespaceInfo? GetNamespace(NamespacePath path)
        {
            NamespaceInfo? curNamespace = null;

            foreach (var entry in path.Entries)
            {
                if (curNamespace == null) // 처음
                {
                    if (!globalNamespacesById.TryGetValue(entry, out curNamespace))
                        return null;
                }
                else
                {
                    curNamespace = curNamespace.GetNamespaceInfo(entry);
                    if (curNamespace == null)
                        return null;
                }
            }

            Debug.Assert(curNamespace != null);
            return curNamespace;
        }

        public ItemInfo? GetGlobalItem(NamespacePath path, ItemPathEntry elem)
        {
            if (path.Entries.Length == 0)
                return globalItemsByElem.GetValueOrDefault(elem);

            var curNamespace = GetNamespace(path);
            if (curNamespace == null)
                return null;
            
            return curNamespace.GetItem(elem);
        }

        public IEnumerable<FuncInfo> GetGlobalFuncs(NamespacePath path, Name funcName)
        {
            if (path.Entries.Length == 0)
                return globalFuncsByName.GetValues(funcName);

            var curNamespace = GetNamespace(path);

            if (curNamespace == null)
                return Enumerable.Empty<FuncInfo>();
                
            return curNamespace.GetFuncs(funcName);
        }
    }
}