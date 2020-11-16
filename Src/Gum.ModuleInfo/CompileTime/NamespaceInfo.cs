using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Immutable;
using Gum.Misc;

namespace Gum.CompileTime
{
    // 네임스페이스는 내용물이 거의 moduleInfo와 같다
    public partial class NamespaceInfo
    {
        public NamespaceId Id { get; }
        private ImmutableDictionary<NamespaceId, NamespaceInfo> namespaces;
        private Dictionary<ItemPathEntry, ItemInfo> itemsByElem;
        private MultiDict<Name, FuncInfo> funcsByName;
        private Dictionary<Name, VarInfo> varsByName;

        public NamespaceInfo(NamespaceId id, IEnumerable<NamespaceInfo> namespaces, IEnumerable<ItemInfo> items)
        {
            Id = id;
            this.namespaces = namespaces.ToImmutableDictionary(ns => ns.Id);
            this.itemsByElem = new Dictionary<ItemPathEntry, ItemInfo>();
            this.funcsByName = new MultiDict<Name, FuncInfo>();
            this.varsByName = new Dictionary<Name, VarInfo>();

            foreach (var item in items)
            {
                itemsByElem.Add(item.GetLocalId(), item);

                if (item is FuncInfo func)
                    this.funcsByName.Add(func.GetLocalId().Name, func);

                else if (item is VarInfo variable)
                    this.varsByName.Add(variable.GetLocalId().Name, variable);
            }
        }

        public NamespaceInfo? GetNamespaceInfo(NamespaceId entry)
        {
            return namespaces.GetValueOrDefault(entry);
        }

        // Path조각으로 멤버 검색
        public ItemInfo? GetItem(ItemPathEntry id)
        {
            return itemsByElem.GetValueOrDefault(id);
        }
        
        // 이름으로 함수들 검색하기 (이름은 같고 타입파라미터, 파라미터만 다른 함수들이 있다)
        public IEnumerable<FuncInfo> GetFuncs(Name funcName)
        {
            return funcsByName.GetValues(funcName);
        }

        // 이름으로 변수 검색
        public VarInfo? GetVar(Name varName)
        {
            return varsByName.GetValueOrDefault(varName);
        }
    }
}
