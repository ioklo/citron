using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Gum.StaticAnalysis
{
    // TypeSkeleton 정보, 이름별 TypeId와 부속타입 정보, 타입 파라미터 개수
    public class TypeSkeleton
    {
        public ModuleItemId TypeId { get; }
        private Dictionary<ModuleItemIdElem, ModuleItemId> memberTypeIds;
        private ImmutableHashSet<string> enumElemNames;

        public TypeSkeleton(ModuleItemId typeId, IEnumerable<string> enumElemNames)
        {
            TypeId = typeId;
            memberTypeIds = new Dictionary<ModuleItemIdElem, ModuleItemId>();
            this.enumElemNames = enumElemNames.ToImmutableHashSet();
        }

        public bool GetMemberTypeId(string name, int typeParamCount, [NotNullWhen(returnValue: true)] out ModuleItemId? outTypeId)
        {
            return memberTypeIds.TryGetValue(new ModuleItemIdElem(name, typeParamCount), out outTypeId);
        }

        public bool ContainsEnumElem(string name)
        {
            return enumElemNames.Contains(name);
        }

        public void AddMemberTypeId(string name, int typeParamCount, ModuleItemId typeId)
        {
            Debug.Assert(!enumElemNames.Contains(name));
            memberTypeIds.Add(new ModuleItemIdElem(name, typeParamCount), typeId);
        }
    }
}
