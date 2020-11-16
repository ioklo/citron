using Gum.Misc;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Gum.CompileTime
{
    public partial class ScriptModuleInfo : IModuleInfo
    {
        private Dictionary<NamespaceId, NamespaceInfo> namespacesById;
        private Dictionary<ItemPathEntry, ItemInfo> itemsByElem;
        private MultiDict<Name, FuncInfo> funcsByName;
        private Dictionary<Name, VarInfo> varsByName;

        public ScriptModuleInfo(IEnumerable<NamespaceInfo> namespaces, IEnumerable<ItemInfo> globalItems)
        {
            this.namespacesById = namespaces.ToDictionary(ns => ns.Id);
            this.itemsByElem = new Dictionary<ItemPathEntry, ItemInfo>();
            this.funcsByName = new MultiDict<Name, FuncInfo>();
            this.varsByName = new Dictionary<Name, VarInfo>();

            foreach(var item in globalItems)
            {
                Debug.Assert(IsGlobalItem(item));

                itemsByElem.Add(item.GetLocalId(), item);

                if (item is FuncInfo func)
                    funcsByName.Add(item.GetLocalId().Name, func);
                else if (item is VarInfo var)
                    varsByName.Add(item.GetLocalId().Name, var);
            }
        }

        private bool IsGlobalItem(ItemInfo item)
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
                    if (!namespacesById.TryGetValue(entry, out curNamespace))
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

        public ItemInfo? GetItem(NamespacePath path, ItemPathEntry elem)
        {
            if (path.Entries.Length == 0)
                return itemsByElem.GetValueOrDefault(elem);

            var curNamespace = GetNamespace(path);
            if (curNamespace == null)
                return null;
            
            return curNamespace.GetItem(elem);
        }

        public IEnumerable<FuncInfo> GetFuncs(NamespacePath path, Name funcName)
        {
            if (path.Entries.Length == 0)
                return funcsByName.GetValues(funcName);

            var curNamespace = GetNamespace(path);

            if (curNamespace == null)
                return Enumerable.Empty<FuncInfo>();
                
            return curNamespace.GetFuncs(funcName);
        }

        public VarInfo? GetVar(NamespacePath path, Name varName)
        {
            if (path.Entries.Length == 0)
                return varsByName.GetValueOrDefault(varName);

            var curNamespace = GetNamespace(path);
            if (curNamespace == null)
                return null;
            
            return curNamespace.GetVar(varName);
        }
    }
}