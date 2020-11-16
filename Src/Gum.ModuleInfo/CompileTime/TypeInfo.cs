using Gum.Misc;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Gum.CompileTime
{
    // 타입 정보
    public abstract class TypeInfo : ItemInfo
    {
        ImmutableArray<string> typeParams;
        TypeValue? baseTypeValue;

        Dictionary<ItemPathEntry, ItemInfo> itemsByElem;
        MultiDict<Name, FuncInfo> funcsByName;
        Dictionary<Name, VarInfo> varsByName;

        public TypeInfo(
            ItemId id,
            IEnumerable<string> typeParams,
            TypeValue? baseTypeValue,
            IEnumerable<ItemInfo> items)
            : base(id)
        {
            this.typeParams = typeParams.ToImmutableArray();
            this.baseTypeValue = baseTypeValue;
            this.itemsByElem = new Dictionary<ItemPathEntry, ItemInfo>();
            this.funcsByName = new MultiDict<Name, FuncInfo>();
            this.varsByName = new Dictionary<Name, VarInfo>();

            foreach (var item in items)
            {
                itemsByElem.Add(item.GetLocalId(), item);

                if (item is FuncInfo func)
                    funcsByName.Add(item.GetLocalId().Name, func);
                else if (item is VarInfo var)
                    varsByName.Add(item.GetLocalId().Name, var);
            }
        }

        // 타입 파라미터
        public IReadOnlyList<string> GetTypeParams() => typeParams;

        // 부모 타입
        public TypeValue? GetBaseTypeValue() => baseTypeValue;

        // Path조각으로 멤버 검색
        public ItemInfo? GetItem(ItemPathEntry elem)
        {
            return itemsByElem.GetValueOrDefault(elem);
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
